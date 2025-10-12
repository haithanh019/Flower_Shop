using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowerShop_WebApp.Areas.Admin.Models.Products
{
    public class ProductCreateRequest
    {
        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Giá bán")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn một danh mục.")]
        public Guid CategoryId { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải là số không âm.")]
        public int StockQuantity { get; set; }

        [Display(Name = "Kích hoạt hiển thị?")]
        public bool IsActive { get; set; } = true;

        public SelectList? CategoryList { get; set; }

        [Display(Name = "Hình ảnh sản phẩm")]
        public List<IFormFile>? ImageFiles { get; set; }
    }
}
