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
                NeededMainSkill = 5,
                NeededSubSkills = new [] { 50, 43 },
                NeededDescription = "Looking to improve ML knowledge."


            };
        }
    }
}
