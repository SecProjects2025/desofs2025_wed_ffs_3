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
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

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

// Configuração do cookie de autenticação
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



// Configuração das roles de autorização
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
    options.IdleTimeout = TimeSpan.FromMinutes(100); // Tempo de expiração da sessão
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

// Inicializar roles e usuários
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
