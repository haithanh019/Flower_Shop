namespace BusinessLogic.DTOs.Orders;

public class OrderDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerEmail { get; set; }

    public string Status { get; set; } = "Pending"; // OrderStatus as string
    public string? CustomerNote { get; set; }

    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();

    public decimal SubTotal => Items.Sum(i => i.LineTotal);
    public decimal Total => SubTotal; // add shipping/discount if any

    public DateTime CreatedAt { get; set; }

    // Payment summary
    public string? PaymentStatus { get; set; } // as string
    public string? PaymentMethod { get; set; } // as string
    public DateTime? PaymentDate { get; set; }
    public string? TransactionId { get; set; }
}
