using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Identity;

namespace AMAP.Entities
{
    public class AppUser : IdentityUser
    {
        public string? Name { get; set; }

        public UserType UserType { get; set; }
    }
}
