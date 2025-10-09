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
        public decimal Total => SubTotal; // Sẽ cộng thêm phí ship/giảm giá nếu có
        public DateTime CreatedAt { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
