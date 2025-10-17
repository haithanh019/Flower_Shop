using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Địa chỉ Email")]
        public required string Email { get; set; }
    }
}
