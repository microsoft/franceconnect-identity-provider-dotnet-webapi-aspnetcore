// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace WebApp_IdentityProvider_MFA.Models
{

    public class GenderValidation : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var genderString = Convert.ToString(value);
            // As required by FranceConnect specifications
            return (genderString == "male" || genderString == "female");

        }
    }

}
