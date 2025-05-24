using AMAP.Entities;
using AMAP.Entities.Models.ViewModel;
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

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
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
                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(loginViewModel.Email);
                    if (user == null)
                    {
                        return Unauthorized(new { message = "User not found" });
                    }

                    // Obtem as roles dos users
                    var roles = await _userManager.GetRolesAsync(user);

                    return Ok(new
                    {
                        message = "Login successful",
                        userId = user.Id,
                        role = roles.FirstOrDefault()
                    });
                }
                if (result.IsLockedOut)
                {
                    return StatusCode(423, new { message = "Account locked" });
                }
                else
                {
                    return Unauthorized(new { message = "Invalid login attempt" });
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
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
    }
}
