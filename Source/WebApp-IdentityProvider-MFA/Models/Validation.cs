using System.ComponentModel.DataAnnotations;

namespace WebApp_IdentityProvider_MFA.Models
{

    public class GenderValidation : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var genderString = Convert.ToString(value);
            return (genderString == "male" || genderString == "female");

        }
    }

}
