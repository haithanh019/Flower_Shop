using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Auth
{
    public class ResetPasswordViewModel
    {
        [Required]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
        [Display(Name = "Mã OTP")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải gồm 6 chữ số.")]
        public required string Otp { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [Display(Name = "Mật khẩu mới")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public required string ConfirmNewPassword { get; set; }
    }
}
