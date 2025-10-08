using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Cart;

public class CartMergeRequest
{
    [Required]
    public required string SessionId { get; set; }
}
