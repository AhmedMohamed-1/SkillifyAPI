namespace SkillifyAPI.DTOs.Rating
{
    /// <summary>
    /// Public reviewer info shown on a received review.
    /// </summary>
    public class GetReviewerSummaryDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
    }
}
