namespace BusinessLogic.DTOs.Payments;

public class PaymentDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }

    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "CashOnDelivery";
    public string PaymentStatus { get; set; } = "Pending";

    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; }
}
