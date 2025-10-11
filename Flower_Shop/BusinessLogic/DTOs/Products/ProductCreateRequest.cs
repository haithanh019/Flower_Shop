using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.DTOs.Products;

public class ProductCreateRequest
{
    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public List<IFormFile>? ImageFiles { get; set; }
}
