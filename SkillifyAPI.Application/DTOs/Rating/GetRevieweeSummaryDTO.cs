namespace SkillifyAPI.DTOs.Rating
{
    /// <summary>
    /// Public reviewee info shown on a given review.
    /// </summary>
    public class GetRevieweeSummaryDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
    }
}
