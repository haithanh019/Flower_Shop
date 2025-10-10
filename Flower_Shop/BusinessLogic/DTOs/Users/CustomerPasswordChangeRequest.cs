using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Users
{
    public class CustomerPasswordChangeRequest
    {
        [Required]
        public required string CurrentPassword { get; set; }

        [Required, MinLength(6)]
        public required string NewPassword { get; set; }

        [Required, Compare(nameof(NewPassword))]
        public required string ConfirmNewPassword { get; set; }
    }
}
