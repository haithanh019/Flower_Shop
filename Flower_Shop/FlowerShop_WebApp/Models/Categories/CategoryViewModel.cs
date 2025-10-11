namespace FlowerShop_WebApp.Models.Categories
{
    public class CategoryViewModel
    {
        public Guid CategoryId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
