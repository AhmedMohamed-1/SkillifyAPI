using SkillifyAPI.DTOs.Rating;

namespace SkillifyAPI.Services.RatingService
{
    public interface IRatingService
    {
        Task<GetRatingDTO> SubmitRatingAsync(int reviewerId, SubmitRatingDTO dto, CancellationToken ct = default);
        Task<GetRatingDTO?> GetBySessionIdAsync(int userId, int sessionId, CancellationToken ct = default);
        Task<IEnumerable<GetReceivedReviewDTO>> GetMyReceivedReviewsAsync(int userId, CancellationToken ct = default);
        Task<IEnumerable<GetRatingDTO>> GetMyGivenReviewsAsync(int userId, CancellationToken ct = default);
        Task<IEnumerable<GetReceivedReviewDTO>> GetUserReceivedReviewsAsync(int userId, CancellationToken ct = default);
    }
}
