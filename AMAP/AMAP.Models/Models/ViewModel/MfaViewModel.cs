using System.ComponentModel.DataAnnotations;

namespace AMAP.Entities.Models.ViewModel
{
    public class SetupMfaViewModel
    {
        public string SecretKey { get; set; }
        public string ManualEntryKey { get; set; }
        public string QrCodeUri { get; set; }
    }

    public class VerifyMfaViewModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string VerificationCode { get; set; }
    }

    public class LoginWithMfaViewModel
    {
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string VerificationCode { get; set; }
    }
} 