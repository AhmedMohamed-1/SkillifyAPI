using FluentValidation;
using SkillifyAPI.DTOs.Gifts;

namespace SkillifyAPI.Validators.Gifts
{
    public class GiveGiftCreditsDtoValidator
        : AbstractValidator<GiveGiftCreditsDto>
    {
        public GiveGiftCreditsDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .InclusiveBetween(1, 1000);
        }
    }
}
