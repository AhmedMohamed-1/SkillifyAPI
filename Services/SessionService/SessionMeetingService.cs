using Hangfire;
using SkillifyAPI.DTOs.Session;
using SkillifyAPI.ZegoService;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.SessionRepository;
using SkillifyAPI.Repositories.UserRepository;

using SkillifyAPI.Services.CreditService;

namespace SkillifyAPI.Services.SessionService
{
    public class SessionMeetingService : ISessionMeetingService
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IZegoRoomService _zegoRoom;
        private readonly ICreditService _creditService;

        public SessionMeetingService(
            ISessionRepository sessionRepository,
            IUserRepository userRepository,
            IZegoRoomService zegoRoom,
            ICreditService creditService)
        {
            _sessionRepository = sessionRepository;
            _userRepository = userRepository;
            _zegoRoom = zegoRoom;
            _creditService = creditService;
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

            if (session.Status is SessionStatus.Completed or SessionStatus.Cancelled or SessionStatus.Declined)
                return;

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

            session.Status = SessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;

            await _sessionRepository.AddEventAsync(new SessionEvent
            {
                SessionId = sessionId,
                UserId = session.HelperId,
                Type = SessionStatus.Completed,
                CreatedAt = DateTime.UtcNow
            });

            if (session.EscrowHold != null && session.EscrowHold.Status == EscrowStatus.Held && session.Status == SessionStatus.Completed)
            {
                session.EscrowHold.Status = EscrowStatus.Released;
                session.EscrowHold.ReleasedAt = DateTime.UtcNow;

                var helper = await _userRepository.GetUserByIdAsync(session.HelperId);
                if (helper != null)
                {
                    await _creditService.AddCreditsAsync(
                        session.HelperId,
                        session.EscrowHold.CreditsHeld,
                        TransactionType.EscrowRelease,
                        session.Id);
                }
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

        // --- Session Lifecycle Methods ---

        public async Task<GetSessionDTO> RequestSessionAsync(int requesterId, RequestHelpDTO dto, CancellationToken ct = default)
        {
            var scheduledAt = EnsureUtc(dto.ScheduledAt);
            if (scheduledAt <= DateTime.UtcNow)
            {
                throw new ArgumentException("Session schedule time must be in the future (UTC).");
            }

            int creditCost = ValidateDuration(dto.DurationMinutes);

            if (requesterId == dto.HelperId)
            {
                throw new ArgumentException("You cannot request a session with yourself.");
            }

            var helper = await _userRepository.GetUserByIdAsync(dto.HelperId, ct);
            if (helper == null)
            {
                throw new KeyNotFoundException("The requested helper user does not exist.");
            }

            var requester = await _userRepository.GetUserByIdAsync(requesterId, ct);
            if (requester == null)
            {
                throw new KeyNotFoundException("Requester user not found.");
            }

            var skillExists = await _userRepository.MainSkillExistsAsync(dto.MainSkillId, ct);
            if (!skillExists)
            {
                throw new KeyNotFoundException("The specified skill does not exist.");
            }

            if (requester.CreditBalance < creditCost)
            {
                throw new InvalidOperationException($"Insufficient credits. Required: {creditCost}, Available: {requester.CreditBalance}.");
            }

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
            if (scheduledAt <= DateTime.UtcNow)
            {
                throw new ArgumentException("Session schedule time must be in the future (UTC).");
            }

            int creditCost = ValidateDuration(dto.DurationMinutes);

            if (helperId == dto.RequesterId)
            {
                throw new ArgumentException("You cannot offer a session to yourself.");
            }

            var requester = await _userRepository.GetUserByIdAsync(dto.RequesterId, ct);
            if (requester == null)
            {
                throw new KeyNotFoundException("The requested recipient does not exist.");
            }

            var helper = await _userRepository.GetUserByIdAsync(helperId, ct);
            if (helper == null)
            {
                throw new KeyNotFoundException("Helper user not found.");
            }

            var skillExists = await _userRepository.MainSkillExistsAsync(dto.MainSkillId, ct);
            if (!skillExists)
            {
                throw new KeyNotFoundException("The specified skill does not exist.");
            }

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

            if (session.Status != SessionStatus.Pending && session.Status != SessionStatus.ReOffered)
            {
                throw new InvalidOperationException("Only pending or re-offered sessions can be accepted.");
            }

            if (session.Status == SessionStatus.Pending)
            {
                // Determine creator from navigation/events
                // If EscrowHold is already set, this was a "Request Help" flow initiated by the Requester
                bool isRequestHelpFlow = session.EscrowHold != null && session.EscrowHold.Status == EscrowStatus.Held;

                if (isRequestHelpFlow)
                {
                    if (session.HelperId != userId)
                    {
                        throw new UnauthorizedAccessException("Only the helper can accept this session request.");
                    }
                }
                else
                {
                    if (session.RequesterId != userId)
                    {
                        throw new UnauthorizedAccessException("Only the requester can accept this session offer.");
                    }
                }
            }
            else if (session.Status == SessionStatus.ReOffered)
            {
                // Find the user who rescheduled last
                var lastEvent = session.SessionEvents
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefault(e => e.Type == SessionStatus.ReOffered);

                if (lastEvent != null && lastEvent.UserId == userId)
                {
                    throw new InvalidOperationException("You cannot accept your own reschedule proposal.");
                }

                if (session.RequesterId != userId && session.HelperId != userId)
                {
                    throw new UnauthorizedAccessException("You are not a participant in this session.");
                }
            }

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

            if (session.HelperId != helperId)
            {
                throw new UnauthorizedAccessException("You are not authorized to decline this session request.");
            }

            if (session.Status != SessionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending sessions can be declined.");
            }

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

            if (session.RequesterId != userId && session.HelperId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this session.");
            }

            if (session.Status != SessionStatus.Pending && session.Status != SessionStatus.Accepted && session.Status != SessionStatus.ReOffered)
            {
                throw new InvalidOperationException("Only pending, accepted, or re-offered sessions can be cancelled.");
            }

            if (session.Status == SessionStatus.Accepted)
            {
                CancelScheduledJobs(session);
            }

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

            if (session.RequesterId != userId && session.HelperId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to reschedule this session.");
            }

            if (session.Status != SessionStatus.Pending && session.Status != SessionStatus.Accepted && session.Status != SessionStatus.ReOffered)
            {
                throw new InvalidOperationException("You can only reschedule pending, accepted, or re-offered sessions.");
            }

            var scheduledAt = EnsureUtc(newScheduledAt);
            if (scheduledAt <= DateTime.UtcNow)
            {
                throw new ArgumentException("Reschedule time must be in the future (UTC).");
            }

            // If it was already accepted, we must cancel current scheduled Hangfire jobs and clear Zego room
            if (session.Status == SessionStatus.Accepted)
            {
                CancelScheduledJobs(session);
                session.HangfireOpenJobId = null;
                session.HangfireCloseJobId = null;
                session.ZegoRoomId = null;
                session.AcceptedAt = null;
            }

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

        public async Task<GetSessionDTO> GetSessionByIdAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdWithDetailsAsync(sessionId, ct)
                ?? throw new KeyNotFoundException("Session not found.");

            if (session.RequesterId != userId && session.HelperId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this session.");
            }

            return MapToDto(session);
        }

        public async Task<IEnumerable<GetSessionDTO>> GetRequestedSessionsAsync(int requesterId, CancellationToken ct = default)
        {
            var sessions = await _sessionRepository.GetRequestedSessionsAsync(requesterId, ct);
            return sessions.Select(MapToDto);
        }

        public async Task<IEnumerable<GetSessionDTO>> GetReceivedSessionsAsync(int helperId, CancellationToken ct = default)
        {
            var sessions = await _sessionRepository.GetReceivedSessionsAsync(helperId, ct);
            return sessions.Select(MapToDto);
        }

        private static GetSessionDTO MapToDto(Session s)
        {
            return new GetSessionDTO
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
