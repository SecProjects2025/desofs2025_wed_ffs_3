using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace AMAP.Api.Validators
{
    public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var errors = new List<IdentityError>();
            if (password.Length > 128)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooLong",
                    Description = "Password must not exceed 128 characters."
                });
            }
            var normalizedPassword = Regex.Replace(password, @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(1));
            if (normalizedPassword.Length < 12)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooShort",
                    Description = "Password must be at least 12 characters long after normalizing spaces."
                });
            }
            return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
}