using System.ComponentModel.DataAnnotations;

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

    // Upload results from FE
    public ICollection<string>? ImageUrls { get; set; }
    public ICollection<string>? ImagePublicIds { get; set; }
}
