using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Cart;

public class CartRemoveItemRequest
{
    [Required]
    public Guid CartItemId { get; set; }
}
