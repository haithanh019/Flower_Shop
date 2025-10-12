using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities
{
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Precision(18, 2)]
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? TransactionId { get; set; }
    }

    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng")]
        CashOnDelivery = 0,

        [Display(Name = "Thanh toán qua PayOS")]
        PayOS = 1,
    }

    public enum PaymentStatus
    {
        [Display(Name = "Chờ thanh toán")]
        Pending = 0,

        [Display(Name = "Đã thanh toán")]
        Accepted = 1,

        [Display(Name = "Đã giao hàng")]
        Delivered = 2,

        [Display(Name = "Đã hoàn tiền")]
        Refunded = 3,
    }
}
