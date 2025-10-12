using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string OrderNumber { get; set; } = "";

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [MaxLength(200)]
        public string? ShippingAddress { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Precision(18, 2)]
        public decimal Subtotal { get; set; }

        [Precision(18, 2)]
        public decimal ShippingFee { get; set; }

        [Precision(18, 2)]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }
    }

    public enum OrderStatus
    {
        [Display(Name = "Chờ xử lý")]
        Pending = 0,

        [Display(Name = "Đã xác nhận")]
        Confirmed = 1,

        [Display(Name = "Đang giao hàng")]
        Shipping = 2,

        [Display(Name = "Hoàn thành")]
        Completed = 3,

        [Display(Name = "Đã hủy")]
        Cancelled = 4,
    }
}
