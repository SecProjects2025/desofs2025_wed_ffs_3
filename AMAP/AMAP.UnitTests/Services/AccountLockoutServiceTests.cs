using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AMAP.Entities;
using AMAP.Entities.Utilities;

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

    // Adicionar mais testes para lockout progressivo e reset
} 