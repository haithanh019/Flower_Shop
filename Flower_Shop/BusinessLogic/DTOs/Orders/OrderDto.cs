namespace BusinessLogic.DTOs.Orders
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerEmail { get; set; }
        public string Status { get; set; } = "Pending";
        public string? CustomerNote { get; set; }

        // Thêm thuộc tính ShippingAddress
        public string? ShippingAddress { get; set; }

        public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
        public decimal Total => SubTotal;
        public DateTime CreatedAt { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
    }
}
