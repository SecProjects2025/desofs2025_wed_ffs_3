using AMAP.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AMAP.Api.Middleware
{
    public class MfaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MfaMiddleware> _logger;

        // Endpoints that don't require MFA verification
        private readonly HashSet<string> _excludedEndpoints = new HashSet<string>
        {
            "/api/twofactorauth/setup",
            "/api/twofactorauth/verify",
            "/api/twofactorauth/login-verify",
            "/api/account/login",
            "/api/account/logout"
        };

        public MfaMiddleware(RequestDelegate next, ILogger<MfaMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager)
        {
            // Skip middleware for excluded endpoints or non-authenticated users
            if (!context.User.Identity.IsAuthenticated || 
                _excludedEndpoints.Any(endpoint => context.Request.Path.StartsWithSegments(endpoint, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Check if the user is a Manager
            if (context.User.IsInRole("Manager"))
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        // If MFA setup is required or MFA is enabled but not verified for this session
                        if (user.TwoFactorSetupRequired || (user.TwoFactorEnabled && context.Session.GetString("MfaVerified") != "true"))
                        {
                            _logger.LogWarning("Manager access attempt without MFA verification: {UserId}", userId);
                            
                            context.Response.StatusCode = 403; // Forbidden
                            context.Response.ContentType = "application/json";
                            
                            var responseMessage = user.TwoFactorSetupRequired
                                ? "{\"message\":\"MFA setup required\",\"requiresMfaSetup\":true}"
                                : "{\"message\":\"MFA verification required\",\"requiresMfa\":true}";
                            
                            await context.Response.WriteAsync(responseMessage);
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method to add the middleware to the HTTP request pipeline
    public static class MfaMiddlewareExtensions
    {
        public static IApplicationBuilder UseMfaMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MfaMiddleware>();
        }
    }
} 