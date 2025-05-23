using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Repository;
using AMAP.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AMAP.Entities;
using AMAP.DataAccess.DbInitializer;
using AMAP.Entities.Utilities;
using AMAP.Business.Interfaces;
using AMAP.Business;
using AMAP.Business.Components;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Note: File logging was removed to avoid dependency issues in CI/CD pipeline
// To enable file logging, uncomment the line below and ensure Serilog.Extensions.Logging.File package is installed
//builder.Logging.AddFile("Logs/security-{Date}.txt");

// Configure logging levels
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("AMAP.Api.Controllers.AccountController", LogLevel.Information);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000") // URL do frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddScoped<IUnitOfWork, UnityOfWork>();
builder.Services.AddScoped<IProductBc, ProductBc>();
builder.Services.AddScoped<ISubscriptionPeriodBc, SubscriptionPeriodBc>();
builder.Services.AddScoped<IProductOfferBc, ProductOfferBc>();
builder.Services.AddScoped<ISubscriptionBc, SubscriptionBc>();
builder.Services.AddScoped<IPaymentBc, PaymentBc>();
builder.Services.AddScoped<ICatalogBc, CatalogBc>();
builder.Services.AddScoped<IDeliveryBc, DeliveryBc>();
builder.Services.AddScoped<IPaymentsInfoBc, PaymentsInfoBc>();
builder.Services.AddScoped<AccountLockoutService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with password requirements
builder.Services.AddIdentity<AppUser, IdentityRole>(options => 
{
    options.Password.RequiredLength = 12; // Min length 12 chars
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    
    // Progressive lockout after failed login attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Initial 15-minute lockout
    options.Lockout.MaxFailedAccessAttempts = 5; // Lock after 5 failed attempts
    options.Lockout.AllowedForNewUsers = true; // Enable lockout for new users
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddPasswordValidator<CustomPasswordValidator<AppUser>>(); // Custom validator

// Authentication cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/account/login";
    options.LogoutPath = "/api/account/logout";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Authentication cookie expiration time: 30 minutes

    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP
        options.Cookie.SameSite = SameSiteMode.None; // Allow cross-origin
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict; // More secure
    }

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// Authorization roles configuration
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
    options.AddPolicy("Producer", policy => policy.RequireRole("Producer"));
    options.AddPolicy("CoProducer", policy => policy.RequireRole("CoProducer"));
});

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session expiration time: 30 minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP
    options.Cookie.SameSite = SameSiteMode.None;
});

//Email Sender
builder.Services.AddSingleton<EmailSender>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar roles e utilizadores apenas se necessário
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Verifica se há alguma migração pendente antes de inicializar
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        var dbInitializer = new DbInitializer(userManager, roleManager, dbContext);
        dbInitializer.Initialize();
    }
    // Verifica se existem roles
    else if (!roleManager.Roles.Any())
    {
        var dbInitializer = new DbInitializer(userManager, roleManager, dbContext);
        dbInitializer.Initialize();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Custom password validator
public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        var errors = new List<IdentityError>();

        if (password.Length > 128)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooLong",
                Description = "Password must not exceed 128 characters."
            });
        }


        var normalizedPassword = Regex.Replace(password, @"\s+", " ");
        if (normalizedPassword.Length < 12)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Password must be at least 12 characters long after normalizing spaces."
            });
        }

        return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
    }
}
