using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Cart;

public class CartUpdateQtyRequest
{
    [Required]
    public Guid CartItemId { get; set; }

    // <=0 means remove (up to service decision)
    public int Quantity { get; set; }

    public string? SessionId { get; set; }
}
