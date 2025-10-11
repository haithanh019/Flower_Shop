using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FlowerShop_WebApp.Models.Cart;
using FlowerShop_WebApp.Models.Profile; // Thêm using này

namespace FlowerShop_WebApp.Models.Orders
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();

        [Required]
        [Display(Name = "Full Name")]
        public string ShippingFullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string ShippingPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a shipping address.")]
        [Display(Name = "Shipping Address")]
        public Guid SelectedAddressId { get; set; }

        [Display(Name = "Order Note")]
        public string? CustomerNote { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        // Dùng để hiển thị danh sách địa chỉ cho người dùng chọn
        public List<AddressViewModel> SavedAddresses { get; set; } = new List<AddressViewModel>();
    }
}
