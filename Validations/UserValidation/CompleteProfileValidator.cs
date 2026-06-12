using FluentValidation;
using SkillifyAPI.DTOs.User.UserDTO;

namespace SkillifyAPI.Validations.UserValidation
{
    public class CompleteProfileValidator : AbstractValidator<CompleteProfileDTO>
    {
        public CompleteProfileValidator()
        {
            RuleFor(x => x.Bio).MaximumLength(500);
            RuleFor(x => x.JobTitle).MaximumLength(100);
            RuleFor(x => x.OfferedDescription).MaximumLength(1000);
            RuleFor(x => x.NeededDescription).MaximumLength(1000);

            RuleFor(x => x.OfferedMainSkill).GreaterThan(0);
            RuleFor(x => x.OfferedSubSkills).NotEmpty();
            RuleFor(x => x.NeededMainSkill).GreaterThan(0);
            RuleFor(x => x.NeededSubSkills).NotEmpty();
        }
    }
}
