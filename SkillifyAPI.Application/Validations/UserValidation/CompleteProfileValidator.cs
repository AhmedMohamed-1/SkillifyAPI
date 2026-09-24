using FluentValidation;
using SkillifyAPI.DTOs.User.UserDTO;

namespace SkillifyAPI.Validations.UserValidation
{
    public class CompleteProfileValidator : AbstractValidator<CompleteProfileDTO>
    {
            public CompleteProfileValidator()
            {
                RuleFor(x => x)
                    .Must(dto => dto.HasAnyUpdate)
                    .WithMessage("At least one profile field must be provided.");

                RuleFor(x => x.Bio)
                    .MaximumLength(500)
                    .When(x => x.Bio is not null);

                RuleFor(x => x.JobTitle)
                    .MaximumLength(100)
                    .When(x => x.JobTitle is not null);

                RuleFor(x => x.FullName)
                    .MaximumLength(150)
                    .When(x => x.FullName is not null);

                RuleFor(x => x.OfferedDescription)
                    .MaximumLength(1000)
                    .When(x => x.OfferedDescription is not null);


                RuleFor(x => x.OfferedMainSkill)
                    .GreaterThan(0)
                    .When(x => x.OfferedMainSkill.HasValue);


                RuleFor(x => x.OfferedSubSkills)
                    .NotEmpty()
                    .When(x => x.OfferedSubSkills is not null);


                RuleFor(x => x.NeededSkills)
                    .Must(skills =>
                        skills!
                            .Select(x => x.MainSkillId)
                            .Distinct()
                            .Count() == skills.Length)
                    .WithMessage(
                        "Duplicate main skills are not allowed in needed skills.")
                    .When(x => x.NeededSkills is not null);


                RuleForEach(x => x.NeededSkills)
                    .ChildRules(skill =>
                    {
                        skill.RuleFor(x => x.MainSkillId)
                            .GreaterThan(0);

                        skill.RuleFor(x => x.SubSkillIds)
                            .NotEmpty();

                        skill.RuleFor(x => x.Description)
                            .MaximumLength(1000)
                            .When(x => x.Description is not null);
                    })
                    .When(x => x.NeededSkills is not null);
            }
    }
}