using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Categories;

public class CategoryCreateRequest
{
    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }
}
