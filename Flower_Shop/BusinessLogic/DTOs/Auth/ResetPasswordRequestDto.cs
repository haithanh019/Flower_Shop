using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Otp { get; set; }

        [Required, MinLength(6)]
        public required string NewPassword { get; set; }

        [Required, Compare(nameof(NewPassword))]
        public required string ConfirmNewPassword { get; set; }
    }
}
