namespace FlowerShop_WebApp.Models.Cart
{
    public class CartItemViewModel
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductThumbnailUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
