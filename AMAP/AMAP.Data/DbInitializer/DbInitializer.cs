using AMAP.DataAccess.Data;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace AMAP.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            // Apply migrations if not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error as needed
            }

            // Create roles if they don't exist
            CreateRoleIfNotExists(nameof(UserType.Manager));
            CreateRoleIfNotExists(nameof(UserType.Producer));
            CreateRoleIfNotExists(nameof(UserType.CoProducer));

            // No default accounts are created
            // Users must be created through the proper registration process
        }

        // Função para criar a role se não existir
        private void CreateRoleIfNotExists(string roleName)
        {
            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
        }

        // Função para criar o utilizador se não existir
        private string CreateUserIfNotExists(string email, UserType role, string name, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    UserType = role,
                };
                var result = _userManager.CreateAsync(user, password).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user, role.ToString()).GetAwaiter().GetResult();
                }
            }
            return user.Id;
        }
    }
}
