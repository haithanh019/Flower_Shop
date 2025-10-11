namespace FlowerShop_WebApp.Models.Orders
{
    public class OrderItemViewModel
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
