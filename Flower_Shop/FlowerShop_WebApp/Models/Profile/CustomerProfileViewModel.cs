using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Profile
{
    public class CustomerProfileViewModel
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = "";

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
