using FluentValidation;
using SkillifyAPI.DTOs.Rating;
using SkillifyAPI.Helper;
using SkillifyAPI.Models;
using SkillifyAPI.Repositories.RatingRepository;
using SkillifyAPI.Repositories.SessionRepository;

namespace SkillifyAPI.Services.RatingService
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IValidator<SubmitRatingDTO> _submitRatingValidator;

        public RatingService(
            IRatingRepository ratingRepository,
            ISessionRepository sessionRepository,
            IValidator<SubmitRatingDTO> submitRatingValidator)
        {
            _ratingRepository = ratingRepository;
            _sessionRepository = sessionRepository;
            _submitRatingValidator = submitRatingValidator;
        }

        public async Task<GetRatingDTO> SubmitRatingAsync(int reviewerId, SubmitRatingDTO dto, CancellationToken ct = default)
        {
            ValidationHelper.EnsureValid(_submitRatingValidator, dto);

            var session = await _sessionRepository.GetByIdAsync(dto.SessionId, ct);
            if (session == null)
                throw new KeyNotFoundException("Session not found.");

            if (session.RequesterId != reviewerId && session.HelperId != reviewerId)
                throw new UnauthorizedAccessException("You are not a participant in this session.");

            if (session.Status != SessionStatus.Completed)
                throw new InvalidOperationException("You can only rate a completed session.");

            if (await _ratingRepository.ExistsForSessionAsync(dto.SessionId, ct))
                throw new InvalidOperationException("This session has already been rated.");

            var revieweeId = session.RequesterId == reviewerId
                ? session.HelperId
                : session.RequesterId;

            var rating = new Rating
            {
                SessionId = dto.SessionId,
                ReviewerId = reviewerId,
                RevieweeId = revieweeId,
                Score = Math.Round(dto.Score, 1),
                ReviewText = string.IsNullOrWhiteSpace(dto.ReviewText) ? null : dto.ReviewText.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _ratingRepository.AddAsync(rating, ct);
            await _ratingRepository.SaveChangesAsync(ct);

            var created = await _ratingRepository.GetByIdAsync(rating.Id, ct);
            return MapToDto(created!);
        }

        public async Task<GetRatingDTO?> GetBySessionIdAsync(int userId, int sessionId, CancellationToken ct = default)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
            if (session == null)
                throw new KeyNotFoundException("Session not found.");

            if (session.RequesterId != userId && session.HelperId != userId)
                throw new UnauthorizedAccessException("You are not authorized to view this session's rating.");

            var rating = await _ratingRepository.GetBySessionIdAsync(sessionId, ct);
            return rating == null ? null : MapToDto(rating);
        }

        public async Task<IEnumerable<GetReceivedReviewDTO>> GetMyReceivedReviewsAsync(int userId, CancellationToken ct = default)
        {
            var ratings = await _ratingRepository.GetReceivedByUserIdAsync(userId, ct);
            return ratings.Select(MapToReceivedReviewDto);
        }

        public async Task<IEnumerable<GetRatingDTO>> GetMyGivenReviewsAsync(int userId, CancellationToken ct = default)
        {
            var ratings = await _ratingRepository.GetGivenByUserIdAsync(userId, ct);
            return ratings.Select(r => new GetRatingDTO
            {
                Id = r.Id,
                SessionId = r.SessionId,
                Score = r.Score,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt,
                Reviewee = new GetRevieweeSummaryDTO
                {
                    UserId = r.Reviewee.Id,
                    FullName = r.Reviewee.FullName,
                    ProfilePictureUrl = r.Reviewee.ProfilePictureUrl
                }
            });
        }

        public async Task<IEnumerable<GetReceivedReviewDTO>> GetUserReceivedReviewsAsync(int userId, CancellationToken ct = default)
        {
            var ratings = await _ratingRepository.GetReceivedByUserIdAsync(userId, ct);
            return ratings.Select(MapToReceivedReviewDto);
        }

        private static GetRatingDTO MapToDto(Rating rating)
        {
            return new GetRatingDTO
            {
                Id = rating.Id,
                SessionId = rating.SessionId,
                Score = rating.Score,
                ReviewText = rating.ReviewText,
                CreatedAt = rating.CreatedAt,
                Reviewer = new GetReviewerSummaryDTO
                {
                    UserId = rating.Reviewer.Id,
                    FullName = rating.Reviewer.FullName,
                    ProfilePictureUrl = rating.Reviewer.ProfilePictureUrl
                },
                Reviewee = new GetRevieweeSummaryDTO
                {
                    UserId = rating.Reviewee.Id,
                    FullName = rating.Reviewee.FullName,
                    ProfilePictureUrl = rating.Reviewee.ProfilePictureUrl
                }
            };
        }

        private static GetReceivedReviewDTO MapToReceivedReviewDto(Rating rating)
        {
            return new GetReceivedReviewDTO
            {
                Id = rating.Id,
                SessionId = rating.SessionId,
                Score = rating.Score,
                ReviewText = rating.ReviewText,
                CreatedAt = rating.CreatedAt,
                Reviewer = new GetReviewerSummaryDTO
                {
                    UserId = rating.Reviewer.Id,
                    FullName = rating.Reviewer.FullName,
                    ProfilePictureUrl = rating.Reviewer.ProfilePictureUrl
                }
            };
        }
    }
}
