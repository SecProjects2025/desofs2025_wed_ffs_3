using AMAP.DataAccess.Repository.IRepository;
using AMAP.DataAccess.Repository;
using AMAP.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AMAP.Entities;
using AMAP.DataAccess.DbInitializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AMAP.Entities.Utilities;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure password hasher with explicit salt size and security parameters
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    // Salt size is 128 bits (16 bytes) - exceeds OWASP requirement of minimum 32 bits
    // Each user gets a unique random salt to prevent salt collisions
    options.IterationCount = 100000; // OWASP C6: PBKDF2 iteration count set to 100,000 as recommended
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3; // Uses PBKDF2 with HMAC-SHA256
});

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme =
//    options.DefaultChallengeScheme =
//    options.DefaultForbidScheme =
//    options.DefaultScheme =
//    options.DefaultSignInScheme =
//    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

//}).AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidIssuer = builder.Configuration["JWT:Issuer"],
//        ValidateAudience = true,
//        ValidAudience = builder.Configuration["JWT:Audience"],
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(
//            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
//        )
//    };
//});

// Configura��o do cookie de autentica��o
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/account/login";
    options.LogoutPath = "/api/account/logout";
    options.SlidingExpiration = true;

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



// Configura��o das roles de autoriza��o
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
    options.IdleTimeout = TimeSpan.FromMinutes(100); // Tempo de expira��o da sess�o
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

// Inicializar roles e usu�rios
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var dbInitializer = new DbInitializer(userManager, roleManager, dbContext);
    dbInitializer.Initialize();
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
