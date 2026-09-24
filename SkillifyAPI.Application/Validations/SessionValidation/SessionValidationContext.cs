using SkillifyAPI.Models;

namespace SkillifyAPI.Validations.SessionValidation
{
    /// <summary>
    /// Carries all the data a <see cref="SessionValidator"/> needs to evaluate
    /// business rules for a given session action.
    /// </summary>
    public class SessionValidationContext
    {
        /// <summary>The session being operated on.</summary>
        public Session Session { get; init; } = null!;

        /// <summary>ID of the user performing the action.</summary>
        public int ActingUserId { get; init; }

        /// <summary>Credit cost for the session duration (pre-calculated).</summary>
        public int CreditCost { get; init; }

        /// <summary>Requester's current credit balance (loaded from DB).</summary>
        public int RequesterCreditBalance { get; init; }

        /// <summary>
        /// The proposed new scheduled time (used for schedule/reschedule validation).
        /// </summary>
        public DateTime? ProposedScheduledAt { get; init; }
    }
}
