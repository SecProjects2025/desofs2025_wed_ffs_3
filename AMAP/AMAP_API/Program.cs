using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Repository;
using AMAP.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Identity;
using AMAP.Entities;
using AMAP.DataAccess.DbInitializer;
using AMAP.Entities.Utilities;
using AMAP.Business.Components;
using AMAP.Business.Interfaces;
using AMAP.Business;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using AMAP.Api.Validators;
using AMAP.Api.Middleware;

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

// Configure Rate Limiting
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });

    rateLimiterOptions.AddFixedWindowLimiter(policyName: "ip_limit", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

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

// Add all business components
builder.Services.AddScoped<IProductBc, ProductBc>();
builder.Services.AddScoped<ISubscriptionPeriodBc, SubscriptionPeriodBc>();
builder.Services.AddScoped<IProductOfferBc, ProductOfferBc>();
builder.Services.AddScoped<ISubscriptionBc, SubscriptionBc>();
builder.Services.AddScoped<IPaymentBc, PaymentBc>();
builder.Services.AddScoped<ICatalogBc, CatalogBc>();
builder.Services.AddScoped<IDeliveryBc, DeliveryBc>();
builder.Services.AddScoped<IPaymentsInfoBc, PaymentsInfoBc>();

builder.Services.AddScoped<AccountLockoutService>();

// Configure database based on environment
if (builder.Environment.EnvironmentName == "Pipeline")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configure Identity with password requirements
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequiredLength = 12;
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
.AddPasswordValidator<CustomPasswordValidator<AppUser>>();

// Configure password hasher
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.IterationCount = 100000;
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
});

// Authentication cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/account/login";
    options.LogoutPath = "/api/account/logout";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Authentication cookie expiration time: 30 minutes

    // Settings that apply to ALL environments
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // Environment-specific settings
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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AMAP API",
        Version = "v1",
        Description = "AMAP API for managing products, subscriptions, and payments"
    });
});

var app = builder.Build();

// Inicializar roles e usuários
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
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Pipeline")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AMAP API v1");
        c.RoutePrefix = "swagger";
    });
}

// Add security headers middleware
app.Use(async (context, next) =>
{
    // Set security headers for all responses
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:;";
        context.Response.Headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Set cache control headers
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
        
        return Task.CompletedTask;
    });

    await next();
});

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseHsts();

app.UseAuthentication();
app.UseSession();
app.UseMfaMiddleware();
app.UseAuthorization();

// Add health check endpoint
app.MapGet("/", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Add rate limiting middleware
app.UseRateLimiter();

app.MapControllers();
app.Run();
