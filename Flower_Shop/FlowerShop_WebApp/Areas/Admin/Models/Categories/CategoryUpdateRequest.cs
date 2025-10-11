using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Areas.Admin.Models.Categories
{
    public class CategoryUpdateRequest
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
