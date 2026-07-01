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

            RuleFor(x => x.OfferedMainSkill).GreaterThan(0);
            RuleFor(x => x.OfferedSubSkills).NotEmpty();

            RuleFor(x => x.NeededSkills).NotEmpty();
            RuleFor(x => x.NeededSkills)
                .Must(skills => skills.Select(s => s.MainSkillId).Distinct().Count() == skills.Length)
                .WithMessage("Duplicate main skills are not allowed in needed skills.");

            RuleForEach(x => x.NeededSkills).ChildRules(skill =>
            {
                skill.RuleFor(s => s.MainSkillId).GreaterThan(0);
                skill.RuleFor(s => s.SubSkillIds).NotEmpty();
                skill.RuleFor(s => s.Description).MaximumLength(1000);
            });
        }
    }
}
