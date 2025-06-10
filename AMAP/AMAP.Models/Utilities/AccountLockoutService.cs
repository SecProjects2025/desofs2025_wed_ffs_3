using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AMAP.Entities.Utilities
{
    public class AccountLockoutService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AccountLockoutService> _logger;

        public AccountLockoutService(UserManager<AppUser> userManager, ILogger<AccountLockoutService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<(bool IsLocked, TimeSpan? LockoutDuration, int FailedAttempts)> HandleFailedLoginAttemptAsync(AppUser user)
        {
            if (user == null)
                return (false, null, 0);

            await _userManager.AccessFailedAsync(user);

            int failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            
            // Check if the account is locked
            DateTimeOffset? lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            bool isLocked = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;
            
            TimeSpan? lockoutDuration = null;
            if (isLocked && lockoutEnd.HasValue)
            {
                lockoutDuration = lockoutEnd.Value - DateTimeOffset.UtcNow;
            }

            // Implement progressive lockout based on the number of failed attempts
            if (failedAttempts >= 5 && failedAttempts < 10)
            {
                // After 5 failures: 15-minute lockout (already configured in Identity)
                _logger.LogWarning("User {UserId} locked out for 15 minutes after {FailedAttempts} failed login attempts", 
                    user.Id, failedAttempts);
            }
            else if (failedAttempts >= 10 && failedAttempts < 15)
            {
                // After 10 failures: 30-minute lockout
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(30));
                _logger.LogWarning("User {UserId} locked out for 30 minutes after {FailedAttempts} failed login attempts", 
                    user.Id, failedAttempts);
            }
            else if (failedAttempts >= 15)
            {
                // After 15 failures: 60-minute lockout
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(60));
                _logger.LogWarning("User {UserId} locked out for 60 minutes after {FailedAttempts} failed login attempts", 
                    user.Id, failedAttempts);
            }

            lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            isLocked = lockoutEnd.HasValue && lockoutEnd.Value > DateTimeOffset.UtcNow;
            
            if (isLocked && lockoutEnd.HasValue)
            {
                lockoutDuration = lockoutEnd.Value - DateTimeOffset.UtcNow;
            }

            return (isLocked, lockoutDuration, failedAttempts);
        }

        public async Task ResetFailedLoginAttemptsAsync(AppUser user)
        {
            if (user == null)
                return;

            await _userManager.ResetAccessFailedCountAsync(user);
            _logger.LogInformation("Reset failed login attempts for user {UserId}", user.Id);
        }
    }
} 