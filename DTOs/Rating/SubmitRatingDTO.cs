namespace SkillifyAPI.DTOs.Rating
{
    public class SubmitRatingDTO
    {
        public int SessionId { get; set; }
        public decimal Score { get; set; }
        public string? ReviewText { get; set; }
    }
}
