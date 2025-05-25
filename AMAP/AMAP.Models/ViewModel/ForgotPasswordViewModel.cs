using System.ComponentModel.DataAnnotations;

namespace AMAP.Entities.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}