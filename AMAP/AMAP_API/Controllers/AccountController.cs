using AMAP.Entities;
using AMAP.Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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

                    // Obter os papéis do usuário
                    var roles = await _userManager.GetRolesAsync(user);

                    return Ok(new
                    {
                        message = "Login successful",
                        userId = user.Id,
                        role = roles.FirstOrDefault() // Retorna o primeiro papel, caso o usuário tenha múltiplos
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

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return Ok(new { message = "If your email is registered, you will receive a password reset token." });
            }

            // Generate password reset token only for existing users
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Ok(new
            {
                message = "Password reset token generated successfully",
                token = token,
                email = model.Email
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return Ok(new { message = "If your email is registered, your password has been reset." });
            }

            // Verify the token is valid for this user
            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<AppUser>.ResetPasswordTokenPurpose,
                model.Token
            );

            if (!isValid)
            {
                return BadRequest(new { message = "Invalid token." });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { message = "Your password has been reset successfully." });
            }

            return BadRequest(new { message = "Password reset failed. Please ensure your new password meets the requirements." });
        }
    }
}
