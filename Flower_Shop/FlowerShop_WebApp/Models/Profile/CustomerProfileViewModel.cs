using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Profile
{
    public class CustomerProfileViewModel
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = "";

        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Điạ chỉ")]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();
    }
}
