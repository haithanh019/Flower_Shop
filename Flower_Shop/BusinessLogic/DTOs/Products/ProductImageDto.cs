namespace BusinessLogic.DTOs.Products;

public class ProductImageDto
{
    public Guid ProductImageId { get; set; }
    public required string ImageUrl { get; set; }
    public string? PublicId { get; set; }
    public int SortOrder { get; set; }
}
