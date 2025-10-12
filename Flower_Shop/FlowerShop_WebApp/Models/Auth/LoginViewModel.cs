using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [Display(Name = "Email")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
