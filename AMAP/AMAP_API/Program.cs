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
builder.Logging.AddFilter("AMAP.Business.Components", LogLevel.Information);

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
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddPasswordValidator<CustomPasswordValidator<AppUser>>(); // Custom validator

// Configurao do cookie de autenticao
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/account/login";
    options.LogoutPath = "/api/account/logout";
    options.SlidingExpiration = true;

    // Settings that apply to ALL environments
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Environment-specific settings
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Permitir HTTP
        options.Cookie.SameSite = SameSiteMode.None; // Permitir cross-origin
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Apenas HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Mais seguro
    }

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// Configurao das roles de autorizao
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
    options.IdleTimeout = TimeSpan.FromMinutes(100); // Tempo de expirao da sessao
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Permitir envio em HTTP
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
app.UseHsts();

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
