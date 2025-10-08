namespace BusinessLogic.DTOs.Categories;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
