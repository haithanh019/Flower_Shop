using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Profile
{
    public class CustomerProfileUpdateViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Address")]
        [MaxLength(200)]
        public string? Address { get; set; }
    }
}
