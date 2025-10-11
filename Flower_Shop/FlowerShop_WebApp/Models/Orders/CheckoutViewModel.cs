using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FlowerShop_WebApp.Models.Cart;
using FlowerShop_WebApp.Models.Profile;

namespace FlowerShop_WebApp.Models.Orders
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận.")]
        [Display(Name = "Họ và tên người nhận")]
        public string ShippingFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại người nhận")]
        public string ShippingPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn một địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public Guid SelectedAddressId { get; set; }

        [Display(Name = "Ghi chú cho đơn hàng")]
        public string? CustomerNote { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        public List<AddressViewModel> SavedAddresses { get; set; } = new List<AddressViewModel>();
    }
}
