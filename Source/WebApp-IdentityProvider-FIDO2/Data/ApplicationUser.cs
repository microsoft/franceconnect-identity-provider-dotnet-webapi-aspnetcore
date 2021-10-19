using System;
using System.Collections.Generic;
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
    public DateTime? Birthdate { get; set; }
    [PersonalData]
    public string GivenName { get; set; }
    [PersonalData]
    public string FamilyName { get; set; }
    [PersonalData]
    public string? PreferredName { get; set; }
}

