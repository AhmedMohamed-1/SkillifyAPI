namespace SkillifyAPI.DTOs.Rating
{
    /// <summary>
    /// A review left about the profile user (maps from <see cref="Models.Rating"/>).
    /// </summary>
    public class GetReceivedReviewDTO
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public decimal Score { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
        public GetReviewerSummaryDTO Reviewer { get; set; } = null!;
    }
}
