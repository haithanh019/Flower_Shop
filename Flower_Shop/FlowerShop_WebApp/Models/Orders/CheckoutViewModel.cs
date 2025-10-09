using System.ComponentModel.DataAnnotations;
using FlowerShop_WebApp.Models.Cart;

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

        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Order Note")]
        public string? CustomerNote { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }
}
