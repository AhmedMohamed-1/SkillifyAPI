using SkillifyAPI.Models;

namespace SkillifyAPI.Validations.SessionValidation
{
    /// <summary>
    /// Centralises every business-rule guard for session operations.
    ///
    /// Each <c>Ensure*</c> method reads from a <see cref="SessionValidationContext"/>
    /// and throws a typed exception on failure so the service stays clean.
    ///
    /// Exception types follow the convention used across the rest of the API:
    ///   <see cref="ArgumentException"/>          – bad input value
    ///   <see cref="InvalidOperationException"/>  – wrong session state
    ///   <see cref="UnauthorizedAccessException"/>– caller is not allowed
    ///   <see cref="KeyNotFoundException"/>       – entity not found
    /// </summary>
    public class SessionValidator
    {
        // ── Scheduling ────────────────────────────────────────────────────────

        /// <summary>Scheduled time must be strictly in the future.</summary>
        public void EnsureScheduledAtIsFuture(SessionValidationContext ctx)
        {
            if (ctx.ProposedScheduledAt == null)
                throw new ArgumentException("A scheduled time is required.");

            if (ctx.ProposedScheduledAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("Session schedule time must be in the future (UTC).");
        }

        // ── Identity ──────────────────────────────────────────────────────────

        /// <summary>A user cannot request a session with themselves as helper.</summary>
        public void EnsureNotSelfRequest(int requesterId, int helperId)
        {
            if (requesterId == helperId)
                throw new ArgumentException("You cannot request a session with yourself.");
        }

        /// <summary>A user cannot offer a session to themselves as requester.</summary>
        public void EnsureNotSelfOffer(int helperId, int requesterId)
        {
            if (helperId == requesterId)
                throw new ArgumentException("You cannot offer a session to yourself.");
        }

        // ── Credits ───────────────────────────────────────────────────────────

        /// <summary>Requester must have enough credits to cover the session cost.</summary>
        public void EnsureSufficientCredits(SessionValidationContext ctx)
        {
            if (ctx.RequesterCreditBalance < ctx.CreditCost)
                throw new InvalidOperationException(
                    $"Insufficient credits. Required: {ctx.CreditCost}, " +
                    $"Available: {ctx.RequesterCreditBalance}.");
        }

        // ── Status guards ─────────────────────────────────────────────────────

        /// <summary>Session must be Pending or ReOffered to be accepted.</summary>
        public void EnsureCanBeAccepted(SessionValidationContext ctx)
        {
            var status = ctx.Session.Status;
            if (status != SessionStatus.Pending && status != SessionStatus.ReOffered)
                throw new InvalidOperationException(
                    "Only pending or re-offered sessions can be accepted.");
        }

        /// <summary>Session must be Pending to be declined.</summary>
        public void EnsureCanBeDeclined(SessionValidationContext ctx)
        {
            if (ctx.Session.Status != SessionStatus.Pending)
                throw new InvalidOperationException("Only pending sessions can be declined.");
        }

        /// <summary>Session must be Pending, Accepted, or ReOffered to be cancelled.</summary>
        public void EnsureCanBeCancelled(SessionValidationContext ctx)
        {
            var status = ctx.Session.Status;
            if (status != SessionStatus.Pending
             && status != SessionStatus.Accepted
             && status != SessionStatus.ReOffered)
                throw new InvalidOperationException(
                    "Only pending, accepted, or re-offered sessions can be cancelled.");
        }

        /// <summary>Session must be Pending, Accepted, or ReOffered to be rescheduled.</summary>
        public void EnsureCanBeRescheduled(SessionValidationContext ctx)
        {
            var status = ctx.Session.Status;
            if (status != SessionStatus.Pending
             && status != SessionStatus.Accepted
             && status != SessionStatus.ReOffered)
                throw new InvalidOperationException(
                    "You can only reschedule pending, accepted, or re-offered sessions.");
        }

        // ── Authorisation guards ──────────────────────────────────────────────

        /// <summary>Only the helper may decline a session request.</summary>
        public void EnsureIsHelper(SessionValidationContext ctx)
        {
            if (ctx.Session.HelperId != ctx.ActingUserId)
                throw new UnauthorizedAccessException(
                    "You are not authorized to decline this session request.");
        }

        /// <summary>Acting user must be either requester or helper.</summary>
        public void EnsureIsParticipant(SessionValidationContext ctx, string action = "access")
        {
            var s = ctx.Session;
            if (s.RequesterId != ctx.ActingUserId && s.HelperId != ctx.ActingUserId)
                throw new UnauthorizedAccessException(
                    $"You are not authorized to {action} this session.");
        }

        /// <summary>
        /// Validates who is allowed to accept a Pending session.
        ///   - Request-Help flow (escrow already held) → only the Helper may accept.
        ///   - Offer-Help flow (no escrow yet)         → only the Requester may accept.
        /// </summary>
        public void EnsureCanAcceptPending(SessionValidationContext ctx)
        {
            bool isRequestHelpFlow =
                ctx.Session.EscrowHold != null &&
                ctx.Session.EscrowHold.Status == EscrowStatus.Held;

            if (isRequestHelpFlow)
            {
                if (ctx.Session.HelperId != ctx.ActingUserId)
                    throw new UnauthorizedAccessException(
                        "Only the helper can accept this session request.");
            }
            else
            {
                if (ctx.Session.RequesterId != ctx.ActingUserId)
                    throw new UnauthorizedAccessException(
                        "Only the requester can accept this session offer.");
            }
        }

        /// <summary>
        /// On a ReOffered session the user who proposed the reschedule
        /// cannot be the one to accept it.
        /// </summary>
        public void EnsureCanAcceptReOffered(SessionValidationContext ctx)
        {
            EnsureIsParticipant(ctx, "accept");

            var lastReOffer = ctx.Session.SessionEvents
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefault(e => e.Type == SessionStatus.ReOffered);

            if (lastReOffer != null && lastReOffer.UserId == ctx.ActingUserId)
                throw new InvalidOperationException(
                    "You cannot accept your own reschedule proposal.");
        }

        // ── Composite helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Full accept-guard: picks the right sub-rule based on current session status.
        /// </summary>
        public void EnsureCanAccept(SessionValidationContext ctx)
        {
            EnsureCanBeAccepted(ctx);

            if (ctx.Session.Status == SessionStatus.Pending)
                EnsureCanAcceptPending(ctx);
            else
                EnsureCanAcceptReOffered(ctx);
        }
    }
}
