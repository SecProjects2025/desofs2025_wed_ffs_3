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

            // Aplicar migrations se não tiverem sido aplicadas
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Logar ou tratar o erro conforme necessário
            }

            // Criar roles se não existirem
            CreateRoleIfNotExists(nameof(UserType.Manager));
            CreateRoleIfNotExists(nameof(UserType.Producer));
            CreateRoleIfNotExists(nameof(UserType.CoProducer));

            //Criar os utilizadores
            CreateUserIfNotExists("manager@amap.com", UserType.Manager, "Manager User", "Manager123!");
            CreateUserIfNotExists("producer@amap.com", UserType.Producer, "Producer User", "Producer123!");
            CreateUserIfNotExists("producer2@amap.com", UserType.Producer, "Producer2 User", "Producer123!");
            CreateUserIfNotExists("coproducer@amap.com", UserType.CoProducer, "CoProducer User", "CoProducer123!");
            CreateUserIfNotExists("coproducer2@amap.com", UserType.CoProducer, "CoProducer2 User", "CoProducer123!");
            CreateUserIfNotExists("manager2@amap.com", UserType.Manager, "Manager User2", "Manager123!");

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
            // Verifica se o utilizador já existe pelo email
            var user = _userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
            
            if (user == null)
            {
                // utilizador não existe, então cria um novo
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
            // Não faz nada se o utilizador já existir - mantém a password atual

            return user.Id;
        }
    }
}