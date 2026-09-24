using FluentValidation;
using FluentValidation.Results;

namespace SkillifyAPI.Helper
{
    public static class ValidationHelper
    {
        public static void EnsureValid<T>(IValidator<T> validator, T dto)
        {
            ValidationResult result = validator.Validate(dto);

            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }
    }
}
