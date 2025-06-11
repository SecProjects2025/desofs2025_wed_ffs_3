using AMAP.Entities.Models.ViewModel;
using AMAP.Entities.Utilities;
using AMAP.Entities.ViewModel;
using System.Net;
using System.Net.Http.Json;

namespace AMAP.IntegrationTests.Services;

public class AccountControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_ValidData_ReturnsOk()
    {
        // Arrange
        var client = _client; // No authentication required for registration
        var uniqueEmail = $"test{Guid.NewGuid()}@example.com";
        var registerViewModel = new RegisterViewModel
        {
            Name = "Integration Test User",
            Email = uniqueEmail,
            Password = "StrongPassword123!@#",
            ConfirmPassword = "StrongPassword123!@#",
            UserType = UserType.CoProducer
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/register", registerViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<object>();
        Assert.NotNull(content);
        
        // Verify we can now log in with these credentials
        var loginViewModel = new LoginViewModel
        {
            Email = uniqueEmail,
            Password = "StrongPassword123!@#",
            RememberMe = false
        };
        
        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", loginViewModel);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var client = _client;
        var registerViewModel = new RegisterViewModel
        {
            Name = "Invalid User",
            Email = "invalid-email", // Invalid email format
            Password = "weak", // Too short password
            ConfirmPassword = "weak",
            UserType = UserType.CoProducer
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/register", registerViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = _client;
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        
        var registerViewModel1 = new RegisterViewModel
        {
            Name = "Original User",
            Email = email,
            Password = "StrongPassword123!@#",
            ConfirmPassword = "StrongPassword123!@#",
            UserType = UserType.CoProducer
        };
        
        // First registration should succeed
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel1);
        
        var registerViewModel2 = new RegisterViewModel
        {
            Name = "Duplicate User",
            Email = email, // Same email as before
            Password = "AnotherStrongPwd456!@#",
            ConfirmPassword = "AnotherStrongPwd456!@#",
            UserType = UserType.Producer
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/register", registerViewModel2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var client = _client;
        
        // First register a user
        var email = $"login-test{Guid.NewGuid()}@example.com";
        var password = "LoginTestPassword123!@#";
        
        var registerViewModel = new RegisterViewModel
        {
            Name = "Login Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            UserType = UserType.Producer
        };
        
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel);
        
        var loginViewModel = new LoginViewModel
        {
            Email = email,
            Password = password,
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/login", loginViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Contains("userId", result.Keys);
        Assert.Contains("role", result.Keys);
        Assert.Equal("Login successful", result["message"].ToString());
        Assert.Equal("Producer", result["role"].ToString());
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var client = _client;
        
        // First register a user
        var email = $"invalid-login{Guid.NewGuid()}@example.com";
        var password = "CorrectPassword123!@#";
        
        var registerViewModel = new RegisterViewModel
        {
            Name = "Invalid Login Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            UserType = UserType.CoProducer
        };
        
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel);
        
        var loginViewModel = new LoginViewModel
        {
            Email = email,
            Password = "WrongPassword123!@#", // Wrong password
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/login", loginViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Contains("failedAttempts", result.Keys);
        Assert.Contains("attemptsRemaining", result.Keys);
        Assert.Equal("Invalid login attempt", result["message"].ToString());
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var client = _client;
        var loginViewModel = new LoginViewModel
        {
            Email = $"nonexistent-{Guid.NewGuid()}@example.com",
            Password = "SomePassword123!@#",
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/login", loginViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var client = _client;
        var loginViewModel = new LoginViewModel
        {
            Email = "invalid-email", // Invalid email format
            Password = "", // Empty password
            RememberMe = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/login", loginViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_AuthenticatedUser_ReturnsOk()
    {
        // Arrange
        // First register and login
        var email = $"logout-test{Guid.NewGuid()}@example.com";
        var password = "LogoutTestPassword123!@#";
        
        var client = _client;
        var registerViewModel = new RegisterViewModel
        {
            Name = "Logout Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            UserType = UserType.CoProducer
        };
        
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel);
        
        var loginViewModel = new LoginViewModel
        {
            Email = email,
            Password = password,
            RememberMe = false
        };
        
        await client.PostAsJsonAsync("/api/Account/login", loginViewModel);
        
        // Act
        var response = await client.PostAsync("/api/Account/logout", null);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Equal("Logged off successfully", result["message"]);
    }

    [Fact]
    public async Task ForgotPassword_ExistingEmail_ReturnsOk()
    {
        // Arrange
        var client = _client;
        
        // First register a user
        var email = $"forgot-password{Guid.NewGuid()}@example.com";
        var password = "ForgotPasswordTest123!@#";
        
        var registerViewModel = new RegisterViewModel
        {
            Name = "Forgot Password Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            UserType = UserType.CoProducer
        };
        
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel);
        
        var forgotPasswordViewModel = new ForgotPasswordViewModel
        {
            Email = email
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/forgot-password", forgotPasswordViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Contains("token", result.Keys);
        Assert.Contains("email", result.Keys);
        Assert.Equal("Password reset token generated successfully", result["message"].ToString());
        Assert.Equal(email, result["email"].ToString());
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_ReturnsOkForSecurity()
    {
        // Arrange
        var client = _client;
        var forgotPasswordViewModel = new ForgotPasswordViewModel
        {
            Email = $"nonexistent-{Guid.NewGuid()}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/forgot-password", forgotPasswordViewModel);

        // Assert
        // Should return OK even for non-existent users (security best practice)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Equal("If your email is registered, you will receive a password reset token.", result["message"]);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_ReturnsOk()
    {
        // Arrange
        var client = _client;
        
        // First register a user
        var email = $"reset-password{Guid.NewGuid()}@example.com";
        var password = "ResetPasswordTest123!@#";
        var newPassword = "NewPasswordAfterReset456!@#";
        
        var registerViewModel = new RegisterViewModel
        {
            Name = "Reset Password Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            UserType = UserType.CoProducer
        };
        
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel);
        
        // Request password reset token
        var forgotPasswordViewModel = new ForgotPasswordViewModel
        {
            Email = email
        };
        
        var forgotResponse = await client.PostAsJsonAsync("/api/Account/forgot-password", forgotPasswordViewModel);
        var forgotResult = await forgotResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var token = forgotResult["token"].ToString();
        
        var resetPasswordViewModel = new ResetPasswordViewModel
        {
            Email = email,
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/reset-password", resetPasswordViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Equal("Your password has been reset successfully.", result["message"]);
        
        // Verify we can log in with the new password
        var loginViewModel = new LoginViewModel
        {
            Email = email,
            Password = newPassword,
            RememberMe = false
        };
        
        var loginResponse = await client.PostAsJsonAsync("/api/Account/login", loginViewModel);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var client = _client;
        
        // First register a user
        var email = $"invalid-reset{Guid.NewGuid()}@example.com";
        var password = "InvalidResetTest123!@#";
        
        var registerViewModel = new RegisterViewModel
        {
            Name = "Invalid Reset Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            UserType = UserType.CoProducer
        };
        
        await client.PostAsJsonAsync("/api/Account/register", registerViewModel);
        
        var resetPasswordViewModel = new ResetPasswordViewModel
        {
            Email = email,
            Token = "invalid-token", // Invalid token
            NewPassword = "NewInvalidResetTest456!@#",
            ConfirmPassword = "NewInvalidResetTest456!@#"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Account/reset-password", resetPasswordViewModel);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.Contains("message", result.Keys);
        Assert.Equal("Invalid token.", result["message"]);
    }
}