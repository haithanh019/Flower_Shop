using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Orders;

public class OrderCreateRequest
{
    public Guid? CustomerId { get; set; } // optional if you allow guest checkout
    public string? CustomerNote { get; set; }

    [Required]
    public required string PaymentMethod { get; set; }

    [Required]
    public required string ShippingFullName { get; set; }

    [Required]
    public required string ShippingPhoneNumber { get; set; }

    [Required]
    public Guid AddressId { get; set; }
    public List<OrderItemCreateLine>? Items { get; set; }
}

public class OrderItemCreateLine
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}
