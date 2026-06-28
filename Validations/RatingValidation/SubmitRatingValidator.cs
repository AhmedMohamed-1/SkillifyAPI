using FluentValidation;
using SkillifyAPI.DTOs.Rating;

namespace SkillifyAPI.Validations.RatingValidation
{
    public class SubmitRatingValidator : AbstractValidator<SubmitRatingDTO>
    {
        public SubmitRatingValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0);

            RuleFor(x => x.Score)
                .InclusiveBetween(1.0m, 5.0m)
                .Must(s => s == Math.Round(s, 1))
                .WithMessage("Score must have at most one decimal place.");

            RuleFor(x => x.ReviewText)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.ReviewText));
        }
    }
}
