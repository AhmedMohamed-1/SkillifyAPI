namespace SkillifyAPI.DTOs.Rating
{
    public class GetRatingDTO
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public decimal Score { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
        public GetReviewerSummaryDTO Reviewer { get; set; } = null!;
        public GetRevieweeSummaryDTO Reviewee { get; set; } = null!;
    }
}
