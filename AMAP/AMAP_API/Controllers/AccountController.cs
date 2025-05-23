using AMAP.Entities;
using AMAP.Entities.Models.ViewModel;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly EmailSender _emailSender;
        private readonly AccountLockoutService _lockoutService;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            EmailSender emailSender,
            AccountLockoutService lockoutService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _lockoutService = lockoutService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create the user with the normalized password (spaces handling)
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    UserType = model.UserType
                };

                // Create the user with ASP.NET Identity
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add role based on user type
                    await _userManager.AddToRoleAsync(user, model.UserType.ToString());

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(new { message = "User registered successfully" });
                }

                // If there are any errors, add them to the ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exists before attempting to log in
                var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", loginViewModel.Email);
                    return Unauthorized(new { message = "Invalid login attempt" });
                }

                // Check if the account is locked
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                if (lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow)
                {
                    var remainingLockoutTime = lockoutEnd.Value - DateTimeOffset.UtcNow;
                    _logger.LogWarning("Login attempt for locked account: {Email}, remaining lockout time: {RemainingTime} minutes", 
                        loginViewModel.Email, remainingLockoutTime.TotalMinutes.ToString("0.0"));
                    
                    return StatusCode(423, new { 
                        message = "Account locked", 
                        remainingMinutes = (int)Math.Ceiling(remainingLockoutTime.TotalMinutes) 
                    });
                }

                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Reset failed login attempts counter
                    await _lockoutService.ResetFailedLoginAttemptsAsync(user);

                    // Get user roles
                    var roles = await _userManager.GetRolesAsync(user);

                    // Send login notification email
                    await SendLoginNotificationAsync(user);

                    _logger.LogInformation("User {UserId} ({UserEmail}) logged in successfully", user.Id, user.Email);

                    return Ok(new
                    {
                        message = "Login successful",
                        userId = user.Id,
                        role = roles.FirstOrDefault()
                    });
                }
                else
                {
                    // Manage progressive lockout
                    var (isLocked, lockoutDuration, failedAttempts) = await _lockoutService.HandleFailedLoginAttemptAsync(user);
                    
                    if (isLocked)
                    {
                        _logger.LogWarning("User account locked out after failed login: {Email}, attempts: {Attempts}, duration: {Duration} minutes", 
                            loginViewModel.Email, failedAttempts, lockoutDuration?.TotalMinutes.ToString("0.0"));
                        
                        return StatusCode(423, new { 
                            message = "Account locked", 
                            remainingMinutes = lockoutDuration.HasValue ? (int)Math.Ceiling(lockoutDuration.Value.TotalMinutes) : 15,
                            failedAttempts = failedAttempts
                        });
                    }

                    _logger.LogWarning("Invalid login attempt for user: {Email}, failed attempts: {Attempts}", 
                        loginViewModel.Email, failedAttempts);
                    
                    return Unauthorized(new { 
                        message = "Invalid login attempt", 
                        failedAttempts = failedAttempts,
                        attemptsRemaining = 5 - (failedAttempts % 5) // Shows how many attempts remain until next lockout
                    });
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOff()
        {
            // Get user information before logging out
            var user = await _userManager.GetUserAsync(User);
            
            // Log out
            await _signInManager.SignOutAsync();
            
            // Explicitly clear the session
            HttpContext.Session.Clear();
            
            if (user != null)
            {
                _logger.LogInformation("User {UserId} ({UserEmail}) logged out", user.Id, user.Email);
            }
            
            return Ok(new { message = "Logged off successfully" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Password change attempt failed: Invalid model state");
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Password change attempt failed: User not found");
                return NotFound(new { message = "User not found." });
            }

            _logger.LogInformation("Password change attempt for user {UserId} ({UserEmail})", user.Id, user.Email);

            // Change the password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("Password change failed for user {UserId}: {ErrorCode} - {ErrorDescription}", 
                        user.Id, error.Code, error.Description);
                }
                
                // Log potential brute force attempts
                var failedPasswordChangeAttempts = await _userManager.GetAccessFailedCountAsync(user);
                _logger.LogWarning("User {UserId} has {FailedAttempts} failed password change attempts", 
                    user.Id, failedPasswordChangeAttempts);
                
                return BadRequest(ModelState);
            }

            // Log successful password change
            _logger.LogInformation("Password changed successfully for user {UserId} ({UserEmail})", user.Id, user.Email);

            // Sign in the user again to refresh the authentication cookie
            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(new { message = "Password changed successfully" });
        }

        private async Task SendLoginNotificationAsync(AppUser user)
        {
            try
            {
                // Get device and location information
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                var loginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var subject = "AMAP Security Alert: New Login Detected";
                var body = $@"
                    <h2>New Login Alert</h2>
                    <p>Dear {user.Name},</p>
                    <p>We detected a new login to your AMAP account.</p>
                    <p><strong>Login Details:</strong></p>
                    <ul>
                        <li>Date and Time: {loginTime}</li>
                        <li>IP Address: {ipAddress}</li>
                        <li>Device: {userAgent}</li>
                    </ul>
                    <p>If this was you, you can ignore this message. If you didn't log in recently, please secure your account immediately by changing your password.</p>
                    <p>Thank you for helping keep your account secure.</p>
                    <p>AMAP Security Team</p>
                ";

                await _emailSender.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation("Login notification email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                // Log the error but don't prevent login
                _logger.LogError(ex, "Failed to send login notification email to {Email}", user.Email);
            }
        }
    }
}
