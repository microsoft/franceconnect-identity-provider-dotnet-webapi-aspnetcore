using System.ComponentModel.DataAnnotations;

namespace WebApp_IdentityProvider_MFA.Helpers.Validation
{

    public class GenderValidation : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            string? genderString = Convert.ToString(value);
            return (new[] { "male", "female", "other" }).Contains(genderString);

        }
    }

}
