using AMAP.DataAccess.Data;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SQLitePCL;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private SqliteConnection _connection;

    static CustomWebApplicationFactory()
    {
        try
        {
            Batteries.Init();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar SQLite na factory: {ex.Message}");
        }
    }

    public CustomWebApplicationFactory()
    {
        // Criar e abrir a conexão no construtor
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            RemoveDbContextRegistrations(services);

            // Add SQLite database context
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            }, ServiceLifetime.Singleton);

            // Register Identity services for testing
            // NOTE: Make sure to keep this AFTER the DbContext registration
            if (!services.Any(d => d.ServiceType == typeof(UserManager<AppUser>)))
            {
                services.AddIdentity<AppUser, IdentityRole>(options =>
                {
                    // Simplify password requirements for testing
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders(); // This registers the required token providers
            }

            // Configure authentication for testing
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test",
                    options => { });

            // Configure default schemes
            services.PostConfigureAll<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = "Test";
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });

            // Seed test data
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            try
            {
                var db = scopedServices.GetRequiredService<AppDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                // Create database
                db.Database.EnsureCreated();

                // Seed test data
                SeedTestData(db, userManager, roleManager).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        });
    }

    private void RemoveDbContextRegistrations(IServiceCollection services)
    {
        // Find and remove ALL EF Core related services
        var descriptorsToRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.Name.StartsWith("IDbContextFactory") &&
                 d.ServiceType.GenericTypeArguments.Any(t => t == typeof(AppDbContext))) ||
                d.ServiceType.Name.StartsWith("DbContextPool") ||
                (d.ImplementationType != null &&
                 d.ImplementationType.Name.Contains("EntityFrameworkCore"))
            ).ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        // Also remove SQL Server specific registrations
        var sqlServerRegistrations = services
            .Where(d =>
                d.ImplementationType != null &&
                d.ImplementationType.FullName != null &&
                d.ImplementationType.FullName.Contains("SqlServer"))
            .ToList();

        foreach (var descriptor in sqlServerRegistrations)
        {
            services.Remove(descriptor);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task SeedTestData(AppDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Criar roles
        if (!await roleManager.RoleExistsAsync("Producer"))
            await roleManager.CreateAsync(new IdentityRole("Producer"));

        if (!await roleManager.RoleExistsAsync("CoProducer"))
            await roleManager.CreateAsync(new IdentityRole("CoProducer"));

        if (!await roleManager.RoleExistsAsync("Manager"))
            await roleManager.CreateAsync(new IdentityRole("Manager"));

        // Criar usuários de teste
        if (!await userManager.Users.AnyAsync())
        {
            // Produtor
            var producer = new AppUser
            {
                Id = "testproducer-id",
                UserName = "testproducer@example.com",
                Email = "testproducer@example.com",
                EmailConfirmed = true,
                Name = "Test Producer"
            };
            await userManager.CreateAsync(producer, "TestPassword123!");
            await userManager.AddToRoleAsync(producer, "Producer");

            // Co-produtor
            var coProducer = new AppUser
            {
                Id = "testcopr-id",
                UserName = "testcopr@example.com",
                Email = "testcopr@example.com",
                EmailConfirmed = true,
                Name = "Test CoProducer"
            };
            await userManager.CreateAsync(coProducer, "TestPassword123!");
            await userManager.AddToRoleAsync(coProducer, "CoProducer");

            // Manager
            var manager = new AppUser
            {
                Id = "testmanager-id",
                UserName = "testmanager@example.com",
                Email = "testmanager@example.com",
                EmailConfirmed = true,
                Name = "Test Manager"
            };
            await userManager.CreateAsync(manager, "TestPassword123!");
            await userManager.AddToRoleAsync(manager, "Manager");
        }

        // Adicionar produtos de teste
        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Test Apples",
                    Description = "Fresh test apples",
                    ReferencePrice = 1.25,
                    DeliveryUnits = "kg",
                    Photo = "apples.jpg",
                    ProducerId = "testproducer-id"
                },
                new Product
                {
                    Id = 2,
                    Name = "Test Oranges",
                    Description = "Fresh test oranges",
                    ReferencePrice = 1.50,
                    DeliveryUnits = "kg",
                    Photo = "oranges.jpg",
                    ProducerId = "testproducer-id"
                }
            );
            await db.SaveChangesAsync();
        }

        if (!db.SubscriptionPeriods.Any())
        {
            var now = DateTime.Now.Date; // Use date portion only to avoid time differences

            db.SubscriptionPeriods.AddRange(
                new SubscriptionPeriod
                {
                    Id = 1,
                    Duration = "Monthly",
                    StartDate = now.AddDays(1),
                    EndDate = now.AddMonths(1)
                },
                new SubscriptionPeriod
                {
                    Id = 2,
                    Duration = "Quarterly",
                    StartDate = now.AddDays(1),
                    EndDate = now.AddMonths(3)
                }
            );
            await db.SaveChangesAsync();
            Console.WriteLine("Subscription periods seeded successfully.");
        }

        // Add delivery dates for the subscription periods
        if (!db.DeliveryDates.Any())
        {
            var now = DateTime.Now.Date; // Use date portion only to avoid time differences

            // Define exact dates to use in tests
            var day7 = now.AddDays(7);
            var day14 = now.AddDays(14);
            var day21 = now.AddDays(21);
            var day30 = now.AddDays(30);

            db.DeliveryDates.AddRange(
                new DeliveryDate
                {
                    Id = 1,
                    SubscriptionPeriodId = 1,
                    Date = day7
                },
                new DeliveryDate
                {
                    Id = 2,
                    SubscriptionPeriodId = 1,
                    Date = day14
                },
                new DeliveryDate
                {
                    Id = 3,
                    SubscriptionPeriodId = 1,
                    Date = day21
                },
                new DeliveryDate
                {
                    Id = 4,
                    SubscriptionPeriodId = 1,
                    Date = day30
                },
                // Add dates for the second subscription period
                new DeliveryDate
                {
                    Id = 5,
                    SubscriptionPeriodId = 2,
                    Date = day30
                },
                new DeliveryDate
                {
                    Id = 6,
                    SubscriptionPeriodId = 2,
                    Date = now.AddDays(60)
                },
                new DeliveryDate
                {
                    Id = 7,
                    SubscriptionPeriodId = 2,
                    Date = now.AddDays(90)
                }
            );
            await db.SaveChangesAsync();
            Console.WriteLine("Delivery dates seeded successfully.");
        }

        // Add ProductOffers for testing
        if (!db.ProductOffers.Any())
        {
            var now = DateTime.Now.Date;
            var day7 = now.AddDays(7);
            var day14 = now.AddDays(14);

            db.ProductOffers.AddRange(
                new ProductOffer
                {
                    Id = 1,
                    ProductId = 1, // Reference to Test Apples product
                    SubscriptionPeriodId = 1,
                    Price = 20.00,
                    Quantity = 50,
                    DeliveryDate = day7, // Match a seeded DeliveryDate
                    PaymentType = PaymentType.Money
                },
                new ProductOffer
                {
                    Id = 2,
                    ProductId = 2, // Reference to Test Oranges product
                    SubscriptionPeriodId = 1,
                    Price = 25.00,
                    Quantity = 30,
                    DeliveryDate = day14, // Match a seeded DeliveryDate
                    PaymentType = PaymentType.Money | PaymentType.MBway
                }
            );
            await db.SaveChangesAsync();
            Console.WriteLine("ProductOffers seeded successfully");
        }
    }
}