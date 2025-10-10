using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowerShop_WebApp.Areas.Admin.Models.Products
{
    public class ProductCreateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Please select a category.")]
        public Guid CategoryId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number.")]
        public int StockQuantity { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        // Dùng để hiển thị dropdown, không gửi đi
        public SelectList? CategoryList { get; set; }

        [Display(Name = "Product Images")]
        public List<IFormFile>? ImageFiles { get; set; }
    }
}
