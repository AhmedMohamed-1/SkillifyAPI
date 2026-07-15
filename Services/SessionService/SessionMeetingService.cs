using Hangfire;
using SkillifyAPI.DTOs.Session;
using SkillifyAPI.ZegoService;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.SessionRepository;
using SkillifyAPI.Repositories.UserRepository;
using SkillifyAPI.Services.CreditService;
using SkillifyAPI.Validations.SessionValidation;
using FirebaseAdmin.Auth;
using SkillifyAPI.Repositories.RatingRepository;

namespace SkillifyAPI.Services.SessionService
{
    public class SessionMeetingService : ISessionMeetingService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IZegoRoomService _zegoRoom;
        private readonly ICreditService _creditService;
        private readonly IRatingRepository _ratingRepository;
        private readonly SessionValidator _validator;

        public SessionMeetingService(
            ISessionRepository sessionRepository,
            IUserRepository userRepository,
            IZegoRoomService zegoRoom,
            ICreditService creditService,
            IRatingRepository ratingRepository,
            SessionValidator validator)
        {
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
            _zegoRoom = zegoRoom;
            _creditService = creditService;
            _ratingRepository = ratingRepository;
            _validator = validator;
        }

        // Called when Helper accepts the session request
        public async Task OnSessionAccepted(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId)
                ?? throw new InvalidOperationException("Session not found.");

            session.ScheduledAt = EnsureUtc(session.ScheduledAt);

            // Generate the Zego room only now — unknown to anyone before acceptance
            session.ZegoRoomId = Guid.NewGuid().ToString("N");
            session.Status = SessionStatus.Accepted;
            session.AcceptedAt = DateTime.UtcNow;

            if (session.HangfireCloseJobId != null)
            {
                BackgroundJob.Delete(session.HangfireCloseJobId);
                session.HangfireCloseJobId = null;
            }

            // Only schedule open — CloseSession is chained from OpenSession to avoid races
            session.HangfireOpenJobId = BackgroundJob.Schedule<SessionMeetingService>(
                s => s.OpenSession(sessionId),
                session.ScheduledAt);

            await _sessionRepository.AddEventAsync(new SessionEvent
            {
                SessionId = sessionId,
                UserId = session.HelperId,
                Type = SessionStatus.Accepted,
                CreatedAt = DateTime.UtcNow
            });

            await _sessionRepository.SaveChangesAsync();
        }

        // Hangfire fires this at ScheduledAt (UTC)
        [AutomaticRetry(Attempts = 3)]
        public async Task OpenSession(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.Status != SessionStatus.Accepted) return;

            session.Status = SessionStatus.Active;

            await _sessionRepository.AddEventAsync(new SessionEvent
            {
                SessionId = sessionId,
                UserId = session.HelperId,
                Type = SessionStatus.Active,
                CreatedAt = DateTime.UtcNow
            });

            var closeAt = GetSessionEndUtc(session);
            if (closeAt <= DateTime.UtcNow)
                closeAt = DateTime.UtcNow.AddSeconds(30);

            if (session.HangfireCloseJobId != null)
                BackgroundJob.Delete(session.HangfireCloseJobId);

            session.HangfireCloseJobId = BackgroundJob.Schedule<SessionMeetingService>(
                s => s.CloseSession(sessionId),
                closeAt);

            await _sessionRepository.SaveChangesAsync();
        }

        // Hangfire fires this after the session duration ends (scheduled by OpenSession)
        [AutomaticRetry(Attempts = 3)]
        public async Task CloseSession(int sessionId)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId);
            if (session == null) return;

            if (session.Status is SessionStatus.Completed
                                or SessionStatus.Cancelled
                                or SessionStatus.Declined
                                or SessionStatus.Expired)
                return;

            // If escrow was already released by the webhook (helper joined on time),
            // the session is already Completed — just close the Zego room and return.
            if (session.EscrowHold?.Status == EscrowStatus.Released)
            {
                if (!string.IsNullOrEmpty(session.ZegoRoomId))
                    await _zegoRoom.CloseRoomAsync(session.ZegoRoomId);

                session.Status = SessionStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;
                await _sessionRepository.SaveChangesAsync();
                return;
            }

            if (session.Status == SessionStatus.Accepted)
            {
                session.Status = SessionStatus.Active;
                await _sessionRepository.AddEventAsync(new SessionEvent
                {
                    SessionId = sessionId,
                    UserId = session.HelperId,
                    Type = SessionStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (session.Status != SessionStatus.Active)
            {
                return;
            }

            if (!string.IsNullOrEmpty(session.ZegoRoomId))
                await _zegoRoom.CloseRoomAsync(session.ZegoRoomId);

            // Helper never joined — expire the session and refund the payer
            if (session.EscrowHold != null && session.EscrowHold.Status == EscrowStatus.Held)
            {
                session.EscrowHold.Status = EscrowStatus.Refunded;
                session.EscrowHold.ReleasedAt = DateTime.UtcNow;

                session.Status = SessionStatus.Expired;
                session.CompletedAt = DateTime.UtcNow;

                await _sessionRepository.AddEventAsync(new SessionEvent
                {
                    SessionId = sessionId,
                    UserId = session.HelperId,
                    Type = SessionStatus.Expired,
                    CreatedAt = DateTime.UtcNow,
                    Comment = "Helper did not join the session. Credits refunded to payer."
                });

                // Refund goes back to whoever paid (EscrowHold.RequesterId = the payer)
                await _creditService.AddCreditsAsync(
                    session.EscrowHold.RequesterId,
                    session.EscrowHold.CreditsHeld,
                    TransactionType.Refund,
                    session.Id);
            }
            else
            {
                // Escrow already settled — just mark completed
                session.Status = SessionStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;

                await _sessionRepository.AddEventAsync(new SessionEvent
                {
                    SessionId = sessionId,
                    UserId = session.HelperId,
                    Type = SessionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _sessionRepository.SaveChangesAsync();
        }

        // If session is cancelled — delete the scheduled jobs
        public void CancelScheduledJobs(Session session)
        {
            if (session.HangfireOpenJobId != null)
                BackgroundJob.Delete(session.HangfireOpenJobId);

            if (session.HangfireCloseJobId != null)
                BackgroundJob.Delete(session.HangfireCloseJobId);
        }

        // ── Zego Webhook Handler ────────────────────────────────────────────────

        /// <summary>
        /// Called by the ZegoWebhookController for every room-user event.
        ///
        /// Business rules:
        ///   • "room_user_join" AND the joining user is the Helper
        ///     AND the session is still Active/Accepted (i.e. escrow not yet released)
        ///     AND the helper joins before the halfway point of the session
        ///     → release escrow immediately: transfer credits to the helper.
        ///
        ///   • Any other event is silently ignored (requester joining, leave events, etc.).
        ///
        /// The "helper didn't join" path is handled by CloseSession (Hangfire job):
        /// when it fires and escrow is still Held → Expired + refund.
        /// </summary>
        public async Task HandleWebhookAsync(ZegoWebhookDto dto, CancellationToken ct = default)
        {
            // We only care about join events
            if (!string.Equals(dto.Event, "room_user_join", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(dto.RoomId))
                return;

            // Look up the session by its Zego room ID
            var session = await _sessionRepository.GetByZegoRoomIdAsync(dto.RoomId, ct);
            if (session == null)
                return;

            // Only process Active or Accepted sessions that still have an unreleased escrow
            if (session.Status is not (SessionStatus.Active or SessionStatus.Accepted))
                return;

            if (session.EscrowHold == null || session.EscrowHold.Status != EscrowStatus.Held)
                return;

            // Parse the joining user's ID from the Zego account string
            if (!int.TryParse(dto.UserAccount, out int joiningUserId))
                return;

            // The credit transfer only triggers when the HELPER joins
            if (joiningUserId != session.HelperId)
                return;

            // Guard: helper must join before the halfway point of the session
            var sessionStart = EnsureUtc(session.ScheduledAt);
            var halfwayPoint = sessionStart.AddMinutes(GetDurationMinutes(session) / 2.0);
            var now = DateTime.UtcNow;

            if (now > halfwayPoint)
                // Helper joined too late — CloseSession will expire and refund
                return;

            // ── Release escrow → helper earns credits immediately ──────────────
            session.EscrowHold.Status = EscrowStatus.Released;
            session.EscrowHold.ReleasedAt = now;

            await _sessionRepository.AddEventAsync(new SessionEvent
            {
                SessionId = session.Id,
                UserId = session.HelperId,
                Type = SessionStatus.Active,
                CreatedAt = now,
                Comment = "Helper joined the session. Credits transferred immediately."
            }, ct);

            // Transfer credits to the helper (regardless of who initiated the session)
            await _creditService.AddCreditsAsync(
                session.HelperId,
                session.EscrowHold.CreditsHeld,
                TransactionType.EscrowRelease,
                session.Id,
                ct);

            await _sessionRepository.SaveChangesAsync(ct);
        }

        // --- Session Lifecycle Methods ---

        public async Task<GetSessionDTO> RequestSessionAsync(int requesterId, RequestHelpDTO dto, CancellationToken ct = default)
        {
            var scheduledAt = EnsureUtc(dto.ScheduledAt);
            int creditCost  = ValidateDuration(dto.DurationMinutes);

            _validator.EnsureNotSelfRequest(requesterId, dto.HelperId);
            _validator.EnsureScheduledAtIsFuture(new SessionValidationContext
            {
                ProposedScheduledAt = scheduledAt
            });

            var helper = await _userRepository.GetUserByIdAsync(dto.HelperId, ct)
                ?? throw new KeyNotFoundException("The requested helper user does not exist.");

            var requester = await _userRepository.GetUserByIdAsync(requesterId, ct)
                ?? throw new KeyNotFoundException("Requester user not found.");

            var skillExists = await _userRepository.MainSkillExistsAsync(dto.MainSkillId, ct);
            if (!skillExists)
                throw new KeyNotFoundException("The specified skill does not exist.");

            _validator.EnsureSufficientCredits(new SessionValidationContext
            {
                CreditCost              = creditCost,
                RequesterCreditBalance  = requester.CreditBalance
            });

            // Setup escrow immediately (deduction is handled below)

            var session = new Session
            {
                RequesterId = requesterId,
                HelperId = dto.HelperId,
                MainSkillId = dto.MainSkillId,
                Topic = dto.Topic,
                ProblemDescription = dto.ProblemDescription,
                DurationMinutes = dto.DurationMinutes,
                CreditCost = creditCost,
                Status = SessionStatus.Pending,
                ScheduledAt = scheduledAt,
                CreatedAt = DateTime.UtcNow
            };

            await _sessionRepository.AddAsync(session, ct);
            await _sessionRepository.SaveChangesAsync(ct);

            var escrow = new EscrowHold
            {
                RequesterId = requesterId,
                CreditsHeld = creditCost,
                Status = EscrowStatus.Held,
                HeldAt = DateTime.UtcNow,
                SessionId = session.Id
            };
            await _sessionRepository.AddEscrowHoldAsync(escrow, ct);

            await _creditService.DeductCreditsAsync(
                requesterId,
                creditCost,
                TransactionType.EscrowHold,
                session.Id,
                ct);

            var sessionEvent = new SessionEvent
            {
                UserId = requesterId,
                Type = SessionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                SessionId = session.Id
            };
            await _sessionRepository.AddEventAsync(sessionEvent, ct);

            await _sessionRepository.SaveChangesAsync(ct);

            var createdSession = await _sessionRepository.GetByIdWithDetailsAsync(session.Id, ct);
            return MapToDto(createdSession!);
        }

        public async Task<GetSessionDTO> OfferSessionAsync(int helperId, OfferHelpDTO dto, CancellationToken ct = default)
        {
            var scheduledAt = EnsureUtc(dto.ScheduledAt);
            int creditCost  = ValidateDuration(dto.DurationMinutes);

            _validator.EnsureNotSelfOffer(helperId, dto.RequesterId);
            _validator.EnsureScheduledAtIsFuture(new SessionValidationContext
            {
                ProposedScheduledAt = scheduledAt
            });

            var requester = await _userRepository.GetUserByIdAsync(dto.RequesterId, ct)
                ?? throw new KeyNotFoundException("The requested recipient does not exist.");

            var helper = await _userRepository.GetUserByIdAsync(helperId, ct)
                ?? throw new KeyNotFoundException("Helper user not found.");

            var skillExists = await _userRepository.MainSkillExistsAsync(dto.MainSkillId, ct);
            if (!skillExists)
                throw new KeyNotFoundException("The specified skill does not exist.");

            // Do NOT deduct credits yet. The requester is charged when they accept the offer.
            var session = new Session
            {
                RequesterId = dto.RequesterId,
                HelperId = helperId,
                MainSkillId = dto.MainSkillId,
                Topic = dto.Topic,
                ProblemDescription = dto.ProblemDescription,
                DurationMinutes = dto.DurationMinutes,
                CreditCost = creditCost,
                Status = SessionStatus.Pending,
                ScheduledAt = scheduledAt,
                CreatedAt = DateTime.UtcNow
            };

            await _sessionRepository.AddAsync(session, ct);
            await _sessionRepository.SaveChangesAsync(ct);

            var sessionEvent = new SessionEvent
            {
                UserId = helperId,
                Type = SessionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                SessionId = session.Id
            };
            await _sessionRepository.AddEventAsync(sessionEvent, ct);

            await _sessionRepository.SaveChangesAsync(ct);

            var createdSession = await _sessionRepository.GetByIdWithDetailsAsync(session.Id, ct);
            return MapToDto(createdSession!);
        }

        public async Task AcceptSessionAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct)
                ?? throw new KeyNotFoundException("Session not found.");

            var ctx = new SessionValidationContext { Session = session, ActingUserId = userId };

            _validator.EnsureCanAccept(ctx);

            // Ensure credits are escrowed if not already done (necessary for "Offer Help" flow or re-offers)
            if (session.EscrowHold == null || session.EscrowHold.Status != EscrowStatus.Held)
            {
                var requester = await _userRepository.GetUserByIdAsync(session.RequesterId, ct)
                    ?? throw new InvalidOperationException("Requester user not found.");

                int cost = session.CreditCost;
                var escrow = new EscrowHold
                {
                    RequesterId = session.RequesterId,
                    CreditsHeld = cost,
                    Status = EscrowStatus.Held,
                    HeldAt = DateTime.UtcNow,
                    SessionId = session.Id
                };
                await _sessionRepository.AddEscrowHoldAsync(escrow, ct);

                await _creditService.DeductCreditsAsync(
                    session.RequesterId,
                    cost,
                    TransactionType.EscrowHold,
                    session.Id,
                    ct);
            }

            // Execute actual acceptance (tokens and hangfire schedules)
            await OnSessionAccepted(sessionId);
        }

        public async Task DeclineSessionAsync(int helperId, int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct)
                ?? throw new KeyNotFoundException("Session not found.");

            var ctx = new SessionValidationContext { Session = session, ActingUserId = helperId };

            _validator.EnsureIsHelper(ctx);
            _validator.EnsureCanBeDeclined(ctx);

            session.Status = SessionStatus.Declined;

            // Refund requester (if credits were escrowed)
            if (session.EscrowHold != null && session.EscrowHold.Status == EscrowStatus.Held)
            {
                session.EscrowHold.Status = EscrowStatus.Refunded;
                session.EscrowHold.ReleasedAt = DateTime.UtcNow;

                var requester = await _userRepository.GetUserByIdAsync(session.RequesterId, ct);
                if (requester != null)
                {
                    await _creditService.AddCreditsAsync(
                        session.RequesterId,
                        session.EscrowHold.CreditsHeld,
                        TransactionType.Refund,
                        session.Id,
                        ct);
                }
            }

            await _sessionRepository.AddEventAsync(new SessionEvent
            {
                SessionId = session.Id,
                UserId = helperId,
                Type = SessionStatus.Declined,
                CreatedAt = DateTime.UtcNow
            }, ct);

            await _sessionRepository.SaveChangesAsync(ct);
        }

        public async Task CancelSessionAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct)
                ?? throw new KeyNotFoundException("Session not found.");

            var ctx = new SessionValidationContext { Session = session, ActingUserId = userId };

            _validator.EnsureIsParticipant(ctx, "cancel");
            _validator.EnsureCanBeCancelled(ctx);

            if (session.Status == SessionStatus.Accepted)
                CancelScheduledJobs(session);

            session.Status = SessionStatus.Cancelled;

            // Refund requester
            if (session.EscrowHold != null && session.EscrowHold.Status == EscrowStatus.Held)
            {
                session.EscrowHold.Status = EscrowStatus.Refunded;
                session.EscrowHold.ReleasedAt = DateTime.UtcNow;

                var requester = await _userRepository.GetUserByIdAsync(session.RequesterId, ct);
                if (requester != null)
                {
                    await _creditService.AddCreditsAsync(
                        session.RequesterId,
                        session.EscrowHold.CreditsHeld,
                        TransactionType.Refund,
                        session.Id,
                        ct);
                }
            }

            await _sessionRepository.AddEventAsync(new SessionEvent
            {
                SessionId = session.Id,
                UserId = userId,
                Type = SessionStatus.Cancelled,
                CreatedAt = DateTime.UtcNow
            }, ct);

            await _sessionRepository.SaveChangesAsync(ct);
        }

        public async Task RescheduleSessionAsync(int userId, int sessionId, DateTime newScheduledAt, string? comment = null, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct)
                ?? throw new KeyNotFoundException("Session not found.");

            var scheduledAt = EnsureUtc(newScheduledAt);
            var ctx = new SessionValidationContext
            {
                Session = session,
                ActingUserId = userId,
                ProposedScheduledAt = scheduledAt
            };

            _validator.EnsureIsParticipant(ctx, "reschedule");
            _validator.EnsureCanBeRescheduled(ctx);
            _validator.EnsureScheduledAtIsFuture(ctx);

            // If it was already accepted, we must cancel current scheduled Hangfire jobs and clear Zego room
            if (session.Status == SessionStatus.Accepted)
            {
                CancelScheduledJobs(session);


                session.ScheduledAt = scheduledAt;
                session.Status = SessionStatus.ReOffered;

                await _sessionRepository.AddEventAsync(new SessionEvent
                {
                    SessionId = session.Id,
                    UserId = userId,
                    Type = SessionStatus.ReOffered,
                    CreatedAt = DateTime.UtcNow,
                    Comment = comment
                }, ct);

                await _sessionRepository.SaveChangesAsync(ct);
            }
        }
        public async Task<GetSessionDTO> GetSessionByIdAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct)
                ?? throw new KeyNotFoundException("Session not found.");

            var ctx = new SessionValidationContext { Session = session, ActingUserId = userId };
            _validator.EnsureIsParticipant(ctx, "view");

            var userRating = await _ratingRepository.GetUserRatingForSessionAsync(userId, sessionId, ct);
            return MapToDto(session, userRating);
        }
        public async Task<IEnumerable<GetSessionDTO>> GetRequestedSessionsAsync(int requesterId, CancellationToken ct = default)
        {
            var sessions = (await _sessionRepository.GetRequestedSessionsAsync(requesterId, ct)).ToList();
            if (!sessions.Any())
                return Enumerable.Empty<GetSessionDTO>();

            var sessionIds = sessions.Select(s => s.Id).ToList();
            var ratings = await _ratingRepository.GetUserRatingsForSessionsAsync(requesterId, sessionIds, ct);

            return sessions.Select(s => {
                ratings.TryGetValue(s.Id, out var rating);
                return MapToDto(s, rating);
            });
        }
        public async Task<IEnumerable<GetSessionDTO>> GetReceivedSessionsAsync(int helperId, CancellationToken ct = default)
        {
            var sessions = (await _sessionRepository.GetReceivedSessionsAsync(helperId, ct)).ToList();
            if (!sessions.Any())
                return Enumerable.Empty<GetSessionDTO>();

            var sessionIds = sessions.Select(s => s.Id).ToList();
            var ratings = await _ratingRepository.GetUserRatingsForSessionsAsync(helperId, sessionIds, ct);

            return sessions.Select(s => {
                ratings.TryGetValue(s.Id, out var rating);
                return MapToDto(s, rating);
            });
        }

        private static GetSessionDTO MapToDto(Session s, Rating? userRating = null)
        {
            var dto = new GetSessionDTO
            {
                Id = s.Id,
                RequesterId = s.RequesterId,
                RequesterName = s.Requester?.FullName ?? "Unknown",
                HelperId = s.HelperId,
                HelperName = s.Helper?.FullName ?? "Unknown",
                MainSkillId = s.MainSkillId,
                MainSkillName = s.MainSkills?.Name ?? "Unknown",
                Topic = s.Topic,
                ProblemDescription = s.ProblemDescription,
                DurationMinutes = GetDurationMinutes(s),
                CreditCost = s.CreditCost,
                Status = s.Status,
                ScheduledAt = s.ScheduledAt,
                AcceptedAt = s.AcceptedAt,
                CompletedAt = s.CompletedAt,
                CreatedAt = s.CreatedAt,
                ZegoRoomId = s.ZegoRoomId
            };

            if (userRating != null)
            {
                dto.UserRated = true;
                dto.UserCanRate = false;
                dto.UserRatingScore = userRating.Score;
            }
            else
            {
                dto.UserRated = false;
                dto.UserCanRate = s.Status == SessionStatus.Completed;
                dto.UserRatingScore = null;
            }

            return dto;
        }

        private static DateTime EnsureUtc(DateTime value) =>
            value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };

        private static int ValidateDuration(SessionDuration duration)
        {
            return duration switch
            {
                SessionDuration.FifteenMin => 15,
                SessionDuration.ThirtyMin => 30,
                SessionDuration.SixtyMin => 60,
                _ => throw new ArgumentException("Duration must be 15, 30, or 60 minutes.")
            };
        }

        private static int GetDurationMinutes(Session session) =>
            session.DurationMinutes switch
            {
                SessionDuration.FifteenMin => 15,
                SessionDuration.ThirtyMin => 30,
                SessionDuration.SixtyMin => 60,
                _ => session.CreditCost
            };

        private static DateTime GetSessionEndUtc(Session session) =>
            EnsureUtc(session.ScheduledAt).AddMinutes(GetDurationMinutes(session));
    }
}
