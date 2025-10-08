using System.ComponentModel.DataAnnotations;

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

    public List<string>? ImageUrls { get; set; }
    public List<string>? ImagePublicIds { get; set; }
}
