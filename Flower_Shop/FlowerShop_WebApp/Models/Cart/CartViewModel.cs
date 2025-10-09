namespace FlowerShop_WebApp.Models.Cart
{
    public class CartViewModel
    {
        public Guid CartId { get; set; }
        public IReadOnlyList<CartItemViewModel> Items { get; set; } =
            Array.Empty<CartItemViewModel>();
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
    }
}
