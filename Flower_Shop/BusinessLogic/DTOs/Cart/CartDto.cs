namespace BusinessLogic.DTOs.Cart;

public class CartDto
{
    public Guid CartId { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }

    public IReadOnlyList<CartItemDto> Items { get; set; } = Array.Empty<CartItemDto>();

    public decimal SubTotal => Items.Sum(i => i.LineTotal);
    public DateTime CreatedAt { get; set; }
}
