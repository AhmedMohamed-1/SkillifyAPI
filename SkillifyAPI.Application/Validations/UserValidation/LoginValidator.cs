using FluentValidation;
using SkillifyAPI.DTOs.User.UserDTO;

namespace SkillifyAPI.Validations.UserValidation
{
    public class LoginValidator : AbstractValidator<SignInDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
