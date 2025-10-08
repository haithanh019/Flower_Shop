using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Categories;

public class CategoryUpdateRequest
{
    [Required]
    public required Guid CategoryId { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }
}
