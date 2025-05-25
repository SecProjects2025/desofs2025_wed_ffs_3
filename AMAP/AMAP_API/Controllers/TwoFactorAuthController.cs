using AMAP.Entities;
using AMAP.Entities.Models.ViewModel;
using AMAP.Entities.Utilities;
using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorAuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<TwoFactorAuthController> _logger;
        private const string AuthenticatorAppName = "AMAP";

        public TwoFactorAuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ILogger<TwoFactorAuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet("setup")]
        [Authorize]
        public async Task<IActionResult> SetupMfa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check if user is a Manager, only Managers require MFA
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Manager"))
            {
                return BadRequest(new { message = "MFA is only required for Manager accounts." });
            }

            // Generate a new secret key if one doesn't exist
            if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            {
                // Generate a new secret key
                var secretKey = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
                user.TwoFactorSecretKey = secretKey;
                await _userManager.UpdateAsync(user);
            }

            // Create the setup info for the authenticator app
            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            var setupInfo = twoFactorAuthenticator.GenerateSetupCode(
                AuthenticatorAppName,
                user.Email,
                user.TwoFactorSecretKey,
                false, // Don't use QR code image directly
                3);    // QR code size

            // Return the setup information
            return Ok(new SetupMfaViewModel
            {
                SecretKey = user.TwoFactorSecretKey,
                ManualEntryKey = setupInfo.ManualEntryKey,
                QrCodeUri = setupInfo.QrCodeSetupImageUrl
            });
        }

        [HttpPost("verify")]
        [Authorize]
        public async Task<IActionResult> VerifyMfa([FromBody] VerifyMfaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check if user has a secret key
            if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            {
                return BadRequest(new { message = "MFA is not set up for this account. Please set up MFA first." });
            }

            // Verify the code
            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            var isCodeValid = twoFactorAuthenticator.ValidateTwoFactorPIN(
                user.TwoFactorSecretKey,
                model.VerificationCode);

            if (!isCodeValid)
            {
                _logger.LogWarning("Invalid MFA verification code for user {UserId}", user.Id);
                return BadRequest(new { message = "Invalid verification code." });
            }

            // Enable MFA for the user
            user.TwoFactorEnabled = true;
            user.TwoFactorSetupRequired = false;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("MFA enabled for user {UserId}", user.Id);
            return Ok(new { message = "MFA verification successful. MFA is now enabled for your account." });
        }

        [HttpPost("login-verify")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithMfa([FromBody] LoginWithMfaViewModel model, [FromQuery] string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check if user has MFA enabled
            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecretKey))
            {
                return BadRequest(new { message = "MFA is not enabled for this account." });
            }

            // Verify the code
            var twoFactorAuthenticator = new TwoFactorAuthenticator();
            var isCodeValid = twoFactorAuthenticator.ValidateTwoFactorPIN(
                user.TwoFactorSecretKey,
                model.VerificationCode);

            if (!isCodeValid)
            {
                _logger.LogWarning("Invalid MFA login code for user {UserId}", user.Id);
                return BadRequest(new { message = "Invalid verification code." });
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            // Set session flag to indicate MFA has been verified
            HttpContext.Session.SetString("MfaVerified", "true");

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("User {UserId} logged in with MFA", user.Id);
            return Ok(new
            {
                message = "Login successful",
                userId = user.Id,
                role = roles.FirstOrDefault()
            });
        }

        [HttpGet("status")]
        [Authorize]
        public async Task<IActionResult> GetMfaStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                isMfaEnabled = user.TwoFactorEnabled,
                isMfaSetupRequired = user.TwoFactorSetupRequired
            });
        }

        [HttpPost("disable")]
        [Authorize(Policy = "Manager")]
        public async Task<IActionResult> DisableMfa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Disable MFA
            user.TwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;
            user.TwoFactorSetupRequired = false;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("MFA disabled for user {UserId}", user.Id);
            return Ok(new { message = "MFA has been disabled for your account." });
        }
    }
} 