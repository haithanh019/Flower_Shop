using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }
    }
}
