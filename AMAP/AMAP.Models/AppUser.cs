using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Identity;

namespace AMAP.Entities
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }

        public UserType UserType { get; set; }
        
        // MFA Properties
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecretKey { get; set; }
        public bool TwoFactorSetupRequired { get; set; } = false;
    }
}
