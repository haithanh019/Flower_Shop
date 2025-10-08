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

        [MaxLength(100)]
        public string? TransactionId { get; set; }
    }

    public enum PaymentMethod
    {
        CashOnDelivery = 0,
        VNPay = 1,
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Accepted = 1,
        Delivered = 2,
        Refunded = 3,
    }
}
