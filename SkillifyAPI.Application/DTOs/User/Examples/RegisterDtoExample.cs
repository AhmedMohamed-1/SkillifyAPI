using SkillifyAPI.DTOs.User.UserDTO;
using Swashbuckle.AspNetCore.Filters;

namespace SkillifyAPI.DTOs.User.Examples
{
    public class RegisterDtoExample : IExamplesProvider<RegisterDTO>
    {
        public RegisterDTO GetExamples()
        {
            return new RegisterDTO
            {
                FullName = "Ahmed Mohamed",
                Email = "user@skillify.com",
                Password = "Password@1234",
                ConfirmPassword = "Password@1234"
            };
        }
    }
}
