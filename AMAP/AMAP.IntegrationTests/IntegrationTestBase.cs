using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SQLitePCL;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class IntegrationTestBase : IDisposable
{
    // Inicialize o SQLite diretamente no construtor estático
    static IntegrationTestBase()
    {
        // Inicializar o SQLite diretamente aqui
        try
        {
            Batteries.Init();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar SQLite: {ex.Message}");
            // Em caso de erro, continuar mesmo assim, pois o erro real será exibido nos testes
        }
    }

    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory<Program> _factory;

    public IntegrationTestBase()
    {
        _factory = new CustomWebApplicationFactory<Program>();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    protected HttpClient GetAuthenticatedClient(string userId, string role)
    {
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Configure test authentication
                    services.PostConfigureAll<AuthenticationOptions>(options =>
                    {
                        options.DefaultScheme = "Test";
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    });

                    // Set the user ID and role for this test
                    TestAuthHandler.UserId = userId;
                    TestAuthHandler.Role = role;
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        // Add the authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        return client;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string UserId { get; set; } = "testproducer-id";
    public static string Role { get; set; } = "Producer";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create authentication ticket with the necessary claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Name, $"{Role.ToLower()}@example.com"),
            new Claim(ClaimTypes.Role, Role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        // Debug log
        Console.WriteLine($"TestAuthHandler: Authenticated user {UserId} with role {Role}");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}