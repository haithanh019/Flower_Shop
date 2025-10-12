using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Products
{
    public class ProductImageDeleteRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
