using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.DTOs.Products;

public class ProductUpdateRequest
{
    [Required]
    public Guid ProductId { get; set; }

    public string? Name { get; set; }
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }

    public Guid? CategoryId { get; set; }

    [Range(0, int.MaxValue)]
    public int? StockQuantity { get; set; }

    public bool? IsActive { get; set; }

    public List<IFormFile>? ImageFiles { get; set; }
}
