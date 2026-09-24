using SkillifyAPI.DTOs.User.UserDTO;
using Swashbuckle.AspNetCore.Filters;

namespace SkillifyAPI.DTOs.User.Examples
{
    public class LoginDtoExample : IExamplesProvider<SignInDTO>
    {
        public SignInDTO GetExamples()
        {
            return new SignInDTO
            {
                Email = "user@skillify.com",
                Password = "Password@1234"
            };
        }
    }
}
