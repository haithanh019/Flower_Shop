using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Payments;

public class PaymentUpdateStatusRequest
{
    [Required]
    public Guid PaymentId { get; set; }

    [Required]
    public required string PaymentStatus { get; set; }

    public string? TransactionId { get; set; }
    public DateTime? PaymentDate { get; set; }
}
