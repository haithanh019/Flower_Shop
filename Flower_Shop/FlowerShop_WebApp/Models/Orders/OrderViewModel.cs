namespace FlowerShop_WebApp.Models.Orders
{
    public class OrderViewModel
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerEmail { get; set; }
        public string Status { get; set; } = "Pending";
        public IReadOnlyList<OrderItemViewModel> Items { get; set; } =
            Array.Empty<OrderItemViewModel>();
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
        public decimal Total => SubTotal;
        public DateTime CreatedAt { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Thêm thuộc tính ShippingAddress
        public string? ShippingAddress { get; set; }
    }
}
