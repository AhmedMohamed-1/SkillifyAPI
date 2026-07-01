using SkillifyAPI.DTOs.Skill.SkillDTO;
using SkillifyAPI.DTOs.User.UserDTO;
using Swashbuckle.AspNetCore.Filters;

namespace SkillifyAPI.DTOs.User.Examples
{
    public class CompleteProfileDtoExample : IExamplesProvider<CompleteProfileDTO>
    {
        public CompleteProfileDTO GetExamples()
        {
            return new CompleteProfileDTO
            {
                Bio = "Junior Backend Developer specialized in .NET",
                JobTitle = "Backend Developer",
                OfferedMainSkill = 1,
                OfferedSubSkills = new [] { 1, 7 },
                OfferedDescription = "I can help with ASP.NET Core and APIs.",
                NeededSkills =
                [
                    new UserSkillSelectionDTO
                    {
                        MainSkillId = 5,
                        SubSkillIds = [50, 43],
                        Description = "Looking to improve ML knowledge."
                    },
                    new UserSkillSelectionDTO
                    {
                        MainSkillId = 3,
                        SubSkillIds = [21, 25],
                        Description = "Want to learn UI/UX design fundamentals."
                    }
                ]
            };
        }
    }
}
