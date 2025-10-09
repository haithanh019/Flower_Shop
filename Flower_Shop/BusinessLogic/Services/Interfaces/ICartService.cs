using BusinessLogic.DTOs.Cart;

namespace BusinessLogic.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(Guid? userId, string? sessionId);

        Task<CartDto> AddItemToCartAsync(Guid? userId, CartAddItemRequest request);

        Task<CartDto> UpdateItemQuantityAsync(Guid? userId, CartUpdateQtyRequest request);

        Task<CartDto> RemoveItemFromCartAsync(Guid? userId, CartRemoveItemRequest request);

        Task<CartDto> MergeCartsAsync(Guid userId, CartMergeRequest request);
    }
}
