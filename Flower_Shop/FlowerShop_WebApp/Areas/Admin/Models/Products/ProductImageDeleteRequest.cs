using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Areas.Admin.Models.Products
{
    public class ProductImageDeleteRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
