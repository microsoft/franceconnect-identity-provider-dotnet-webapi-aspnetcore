using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApp_IdentityProvider_MFA.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    public string Gender { get; set; }
    [PersonalData]
    public string GivenName { get; set; }
    [PersonalData]
    public string FamilyName { get; set; }
    [PersonalData]
    public string PreferredName { get; set; }

    [ProtectedPersonalData]
    public DateTime BirthDate { get; set; }

    [StringLength(6, MinimumLength = 5)]
    [Display(Name = "Lieu de naissance au format INSEE a 5 chiffres")]
    [ProtectedPersonalData]
    public string BirthPlace { get; set; }

    [StringLength(6, MinimumLength = 5)]
    [Display(Name = "Pays de naissance au format INSEE a 5 chiffres")]
    [ProtectedPersonalData]
    public string BirthCountry { get; set; }
}

