using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Cart;

public class CartUpdateQtyRequest
{
    [Required]
    public Guid CartItemId { get; set; }

    public int Quantity { get; set; }
}
