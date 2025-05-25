using System.ComponentModel.DataAnnotations;

namespace AMAP.Entities.Models.ViewModel
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}