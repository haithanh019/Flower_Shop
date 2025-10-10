namespace FlowerShop_WebApp.Models.ApiRequest
{
    public class CartAddItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string? SessionId { get; set; }
    }
}
