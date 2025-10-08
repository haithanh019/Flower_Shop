using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Orders;

public class OrderUpdateStatusRequest
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public required string Status { get; set; } // OrderStatus as string
}
