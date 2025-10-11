using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            ErrorMessage = "The {0} must be at least {2} characters long.",
            MinimumLength = 6
        )]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
