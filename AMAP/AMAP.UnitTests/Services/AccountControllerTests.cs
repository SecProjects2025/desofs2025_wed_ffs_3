using AMAP.Api.Controllers;
using AMAP.Entities;
using AMAP.Entities.Models.ViewModel;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace AMAP.UnitTests.Services
{
    public class AccountControllerTests
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly AccountController _accountController;

        public AccountControllerTests()
        {
            // Mock UserManager
            _userManager = Substitute.For<UserManager<AppUser>>(
                Substitute.For<IUserStore<AppUser>>(),
                null, null, null, null, null, null, null, null);

            // Mock SignInManager
            _signInManager = Substitute.For<SignInManager<AppUser>>(
                _userManager,
                Substitute.For<IHttpContextAccessor>(),
                Substitute.For<IUserClaimsPrincipalFactory<AppUser>>(),
                null, null, null, null);

            // Mock RoleManager
            _roleManager = Substitute.For<RoleManager<IdentityRole>>(
                Substitute.For<IRoleStore<IdentityRole>>(),
                null, null, null, null);
                
            // Mock Logger
            _logger = Substitute.For<ILogger<AccountController>>();

            _accountController = new AccountController(_userManager, _signInManager, _roleManager, _logger);

            // Setup controller context
            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            var signInResult = SignInResult.Success;
            _signInManager.PasswordSignInAsync(
                loginViewModel.Email,
                loginViewModel.Password,
                loginViewModel.RememberMe,
                true)
                .Returns(signInResult);

            var user = new AppUser
            {
                Id = "user-id-123",
                Email = loginViewModel.Email,
                UserName = loginViewModel.Email,
                Name = "Test User",
                UserType = UserType.Producer
            };
            _userManager.FindByEmailAsync(loginViewModel.Email).Returns(user);

            var roles = new List<string> { "Producer" };
            _userManager.GetRolesAsync(user).Returns(roles);

            // Act
            var result = await _accountController.Login(loginViewModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Use explicit type conversion instead of dynamic
            var responseDict = Assert.IsAssignableFrom<IDictionary<string, object>>(
                new RouteValueDictionary(okResult.Value));

            Assert.Equal("Login successful", responseDict["message"]);
            Assert.Equal(user.Id, responseDict["userId"]);
            Assert.Equal("Producer", responseDict["role"]);
        }


        [Fact]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var loginViewModel = new LoginViewModel(); // Empty model
            _accountController.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _accountController.Login(loginViewModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "WrongPassword",
                RememberMe = false
            };

            var signInResult = SignInResult.Failed;
            _signInManager.PasswordSignInAsync(
                loginViewModel.Email,
                loginViewModel.Password,
                loginViewModel.RememberMe,
                true)
                .Returns(signInResult);

            // Act
            var result = await _accountController.Login(loginViewModel);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task Login_LockedAccount_ReturnsLocked()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "locked@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            var signInResult = SignInResult.LockedOut;
            _signInManager.PasswordSignInAsync(
                loginViewModel.Email,
                loginViewModel.Password,
                loginViewModel.RememberMe,
                true)
                .Returns(signInResult);

            // Act
            var result = await _accountController.Login(loginViewModel);

            // Assert
            var lockedResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(423, lockedResult.StatusCode);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Email = "notfound@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;
            _signInManager.PasswordSignInAsync(
                loginViewModel.Email,
                loginViewModel.Password,
                loginViewModel.RememberMe,
                true)
                .Returns(signInResult);

            // Return null to simulate user not found
            _userManager.FindByEmailAsync(loginViewModel.Email).ReturnsNull();

            // Act
            var result = await _accountController.Login(loginViewModel);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        [Fact]
        public async Task LogOff_Always_ReturnsOkResult()
        {
            // Arrange
            _signInManager.SignOutAsync().Returns(Task.CompletedTask);

            // Act
            var result = await _accountController.LogOff();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
        
        [Fact]
        public async Task ChangePassword_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordViewModel(); // Empty model
            _accountController.ModelState.AddModelError("CurrentPassword", "Current password is required");

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        
        [Fact]
        public async Task ChangePassword_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            
            // Setup user identity
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user-id-123") };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            // Return null to simulate user not found
            _userManager.GetUserAsync(claimsPrincipal).ReturnsNull();

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        
        [Fact]
        public async Task ChangePassword_PasswordChangeFailed_ReturnsBadRequest()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            
            // Setup user identity
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user-id-123") };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            // Setup user
            var user = new AppUser
            {
                Id = "user-id-123",
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            _userManager.GetUserAsync(claimsPrincipal).Returns(user);
            
            // Setup password change failure
            var identityResult = IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." });
            _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword).Returns(identityResult);
            
            // Setup access failed count
            _userManager.GetAccessFailedCountAsync(user).Returns(1);

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        
        [Fact]
        public async Task ChangePassword_Success_ReturnsOk()
        {
            // Arrange
            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            
            // Setup user identity
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user-id-123") };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _accountController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
            
            // Setup user
            var user = new AppUser
            {
                Id = "user-id-123",
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            _userManager.GetUserAsync(claimsPrincipal).Returns(user);
            
            // Setup password change success
            var identityResult = IdentityResult.Success;
            _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword).Returns(identityResult);
            
            // Setup sign in
            _signInManager.SignInAsync(user, false).Returns(Task.CompletedTask);

            // Act
            var result = await _accountController.ChangePassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            // Verify sign in was called
            await _signInManager.Received(1).SignInAsync(user, false);
        }

        [Fact]
        public async Task Register_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterViewModel(); // Empty model
            _accountController.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _accountController.Register(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        
        [Fact]
        public async Task Register_UserCreationFailed_ReturnsBadRequest()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Test User",
                UserType = UserType.Producer
            };
            
            // Setup user creation failure
            var identityResult = IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email already exists." });
            _userManager.CreateAsync(Arg.Any<AppUser>(), model.Password).Returns(identityResult);

            // Act
            var result = await _accountController.Register(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        
        [Fact]
        public async Task Register_Success_ReturnsOk()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Name = "Test User",
                UserType = UserType.Producer
            };
            
            // Setup user creation success
            var identityResult = IdentityResult.Success;
            _userManager.CreateAsync(Arg.Any<AppUser>(), model.Password).Returns(identityResult);
            
            // Setup role assignment success
            _userManager.AddToRoleAsync(Arg.Any<AppUser>(), model.UserType.ToString()).Returns(identityResult);
            
            // Setup sign in
            _signInManager.SignInAsync(Arg.Any<AppUser>(), false).Returns(Task.CompletedTask);

            // Act
            var result = await _accountController.Register(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            // Verify sign in was called
            await _signInManager.Received(1).SignInAsync(Arg.Any<AppUser>(), false);
        }
    }
}
