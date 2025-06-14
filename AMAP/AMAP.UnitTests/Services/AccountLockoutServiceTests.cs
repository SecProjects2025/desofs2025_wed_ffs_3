using System.Security.Claims;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AMAP.Entities;
using AMAP.Entities.Utilities;
using Microsoft.AspNetCore.Mvc;
using AMAP.Entities.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using AMAP.Api.Controllers;

namespace AMAP.UnitTests.Services
{
    public class AccountLockoutServiceTests
    {
        [Fact]
        public async Task HandleFailedLoginAttemptAsync_UserIsNull_ReturnsFalse()
        {
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var logger = new Mock<ILogger<AccountLockoutService>>();
            var service = new AccountLockoutService(userManager.Object, logger.Object);
            var result = await service.HandleFailedLoginAttemptAsync(null);
            Assert.False(result.IsLocked);
            Assert.Null(result.LockoutDuration);
            Assert.Equal(0, result.FailedAttempts);
        }

        [Fact]
        public async Task ResetFailedLoginAttemptsAsync_UserIsNull_DoesNothing()
        {
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var logger = new Mock<ILogger<AccountLockoutService>>();
            var service = new AccountLockoutService(userManager.Object, logger.Object);
            await service.ResetFailedLoginAttemptsAsync(null);
            // No exception = pass
        }

        [Fact]
        public async Task HandleFailedLoginAttemptAsync_WithLessThan5FailedAttempts_DoesNotLockoutUser()
        {
            // Arrange
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var logger = new Mock<ILogger<AccountLockoutService>>();
            var user = new AppUser { Id = "user-123", Email = "test@example.com" };

            // Setup for 3 failed attempts
            userManager.Setup(u => u.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(u => u.GetAccessFailedCountAsync(user)).ReturnsAsync(3);
            userManager.Setup(u => u.GetLockoutEndDateAsync(user)).ReturnsAsync((DateTimeOffset?)null);

            var service = new AccountLockoutService(userManager.Object, logger.Object);

            // Act
            var result = await service.HandleFailedLoginAttemptAsync(user);

            // Assert
            Assert.False(result.IsLocked);
            Assert.Null(result.LockoutDuration);
            Assert.Equal(3, result.FailedAttempts);
            userManager.Verify(u => u.AccessFailedAsync(user), Times.Once);
            userManager.Verify(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset>()), Times.Never);
        }

        [Fact]
        public async Task HandleFailedLoginAttemptAsync_With5FailedAttempts_LocksUserFor15Minutes()
        {
            // Arrange
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var logger = new Mock<ILogger<AccountLockoutService>>();
            var user = new AppUser { Id = "user-123", Email = "test@example.com" };

            var lockoutTime = DateTimeOffset.UtcNow.AddMinutes(15);

            userManager.Setup(u => u.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(u => u.GetAccessFailedCountAsync(user)).ReturnsAsync(5);

            // First call: no lockout, second call: locked for 15 minutes (Identity default)
            var lockoutCalls = 0;
            userManager.Setup(u => u.GetLockoutEndDateAsync(user)).ReturnsAsync(() =>
            {
                lockoutCalls++;
                // Return null for the first call, then return lockout time
                return lockoutCalls == 1 ? null : lockoutTime;
            });

            var service = new AccountLockoutService(userManager.Object, logger.Object);

            // Act
            var result = await service.HandleFailedLoginAttemptAsync(user);

            // Assert
            Assert.True(result.IsLocked);
            Assert.NotNull(result.LockoutDuration);
            Assert.InRange(result.LockoutDuration.Value.TotalMinutes, 14, 15); // Allow for processing time
            Assert.Equal(5, result.FailedAttempts);
            userManager.Verify(u => u.AccessFailedAsync(user), Times.Once);
        }

        [Fact]
        public async Task HandleFailedLoginAttemptAsync_WithExistingLockout_ReturnsRemainingLockoutTime()
        {
            // Arrange
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var logger = new Mock<ILogger<AccountLockoutService>>();
            var user = new AppUser { Id = "user-123", Email = "test@example.com" };

            var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10); // 10 minutes remaining lockout

            userManager.Setup(u => u.AccessFailedAsync(user)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(u => u.GetAccessFailedCountAsync(user)).ReturnsAsync(7);
            userManager.Setup(u => u.GetLockoutEndDateAsync(user)).ReturnsAsync(lockoutEnd);

            var service = new AccountLockoutService(userManager.Object, logger.Object);

            // Act
            var result = await service.HandleFailedLoginAttemptAsync(user);

            // Assert
            Assert.True(result.IsLocked);
            Assert.NotNull(result.LockoutDuration);
            Assert.InRange(result.LockoutDuration.Value.TotalMinutes, 9, 10); // Allow for processing time
            Assert.Equal(7, result.FailedAttempts);
            userManager.Verify(u => u.AccessFailedAsync(user), Times.Once);
        }

        [Fact]
        public async Task ResetFailedLoginAttemptsAsync_WithUser_ResetsFailedAttempts()
        {
            // Arrange
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
            var logger = new Mock<ILogger<AccountLockoutService>>();
            var user = new AppUser { Id = "user-123", Email = "test@example.com" };

            userManager.Setup(u => u.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

            var service = new AccountLockoutService(userManager.Object, logger.Object);

            // Act
            await service.ResetFailedLoginAttemptsAsync(user);

            // Assert
            userManager.Verify(u => u.ResetAccessFailedCountAsync(user), Times.Once);
        }

        #region AccountController Tests

        // Helper method to create mock IConfiguration
        private static IConfiguration CreateMockConfiguration()
        {
            var configurationMock = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();

            configSectionMock.Setup(s => s["Host"]).Returns("smtp.example.com");
            configSectionMock.Setup(s => s["Port"]).Returns("587");
            configSectionMock.Setup(s => s["Username"]).Returns("username");
            configSectionMock.Setup(s => s["Password"]).Returns("password");
            configSectionMock.Setup(s => s["FromEmail"]).Returns("noreply@example.com");
            configSectionMock.Setup(s => s["FromName"]).Returns("Test System");

            configurationMock.Setup(c => c.GetSection("MailSettings")).Returns(configSectionMock.Object);

            return configurationMock.Object;
        }

        // Helper method to create controller with required dependencies
        private (AccountController controller, Mock<UserManager<AppUser>> userManager, Mock<SignInManager<AppUser>> signInManager, Mock<AccountLockoutService> lockoutService, Mock<EmailSender> emailSender) SetupController()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // Setup SignInManager mock
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            var signInManager = new Mock<SignInManager<AppUser>>(
                userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);

            // Setup RoleManager mock
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);

            // Setup other dependencies
            var logger = new Mock<ILogger<AccountController>>();
            var mockConfig = CreateMockConfiguration();
            var emailSender = new Mock<EmailSender>(mockConfig) { CallBase = true };
            emailSender.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var lockoutService = new Mock<AccountLockoutService>(userManager.Object, Mock.Of<ILogger<AccountLockoutService>>());
            var securityMonitoring = new Mock<SecurityMonitoringService>(
                Mock.Of<ILogger<SecurityMonitoringService>>(),
                userManager.Object,
                emailSender.Object);

            // Create controller with HTTP context
            var controller = new AccountController(
                userManager.Object,
                signInManager.Object,
                roleManager.Object,
                logger.Object,
                emailSender.Object,
                lockoutService.Object,
                securityMonitoring.Object
            );

            // Setup the HttpContext, including Response
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return (controller, userManager, signInManager, lockoutService, emailSender);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var (controller, _, _, _, _) = SetupController();
            controller.ModelState.AddModelError("CurrentPassword", "Required");
            var model = new ChangePasswordViewModel();

            // Act
            var result = await controller.ChangePassword(model, "test-user-agent");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidCurrentPassword_ReturnsBadRequest()
        {
            // Arrange
            var (controller, userManager, _, _, _) = SetupController();

            var model = new ChangePasswordViewModel
            {
                CurrentPassword = "WrongPassword",
                NewPassword = "NewP@ssword123",
                ConfirmPassword = "NewP@ssword123"
            };

            var user = new AppUser { Id = "user-123", Email = "test@example.com" };
            var errors = new List<IdentityError>
        {
            new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." }
        };

            userManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));
            userManager.Setup(um => um.GetAccessFailedCountAsync(user))
                .ReturnsAsync(1);

            // Act
            var result = await controller.ChangePassword(model, "test-user-agent");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var (controller, _, _, _, _) = SetupController();
            controller.ModelState.AddModelError("Email", "Required");
            var model = new LoginViewModel();

            // Act
            var result = await controller.Login(model, "test-user-agent");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion
    }
}