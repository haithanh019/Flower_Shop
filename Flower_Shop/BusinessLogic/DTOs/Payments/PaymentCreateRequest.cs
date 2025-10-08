using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Payments;

public class PaymentCreateRequest
{
    [Required]
    public Guid OrderId { get; set; }

    public decimal? Amount { get; set; } // server can recompute from order

    [Required]
    public required string PaymentMethod { get; set; }
}
