using System.ComponentModel.DataAnnotations;
using FlowerShop_WebApp.Models.Categories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowerShop_WebApp.Areas.Admin.Models
{
    public class ProductEditViewModel
    {
        public Guid ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number.")]
        public int StockQuantity { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        // Dùng để hiển thị dropdown chọn category
        public SelectList? CategoryList { get; set; }
    }
}
