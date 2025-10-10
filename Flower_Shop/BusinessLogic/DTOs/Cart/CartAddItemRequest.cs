using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Cart;

public class CartAddItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}
