using FluentValidation;
using SkillifyAPI.DTOs.Gifts;

namespace SkillifyAPI.Validators.Gifts
{
    public class BulkGiftCreditsDtoValidator
        : AbstractValidator<BulkGiftCreditsDto>
    {
        public BulkGiftCreditsDtoValidator()
        {
            RuleFor(x => x.UserIds)
                .NotEmpty();

            RuleForEach(x => x.UserIds)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .InclusiveBetween(1, 1000);
        }
    }
}
