namespace FlowerShop_WebApp.Models.Products
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<string>? ImageUrls { get; set; }
    }
}
