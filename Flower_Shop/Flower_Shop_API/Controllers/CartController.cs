using System.Security.Claims;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bắt buộc đăng nhập cho tất cả các action
    public class CartController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public CartController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserIdFromClaims();
            var cart = await _facadeService.CartService.GetCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartAddItemRequest request)
        {
            var userId = GetUserIdFromClaims();
            var updatedCart = await _facadeService.CartService.AddItemToCartAsync(userId, request);
            return Ok(updatedCart);
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateItemQuantity([FromBody] CartUpdateQtyRequest request)
        {
            var userId = GetUserIdFromClaims();
            var updatedCart = await _facadeService.CartService.UpdateItemQuantityAsync(
                userId,
                request
            );
            return Ok(updatedCart);
        }

        [HttpPost("items/remove")]
        public async Task<IActionResult> RemoveItemFromCart(
            [FromBody] CartRemoveItemRequest request
        )
        {
            var userId = GetUserIdFromClaims();
            var updatedCart = await _facadeService.CartService.RemoveItemFromCartAsync(
                userId,
                request
            );
            return Ok(updatedCart);
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID claim is invalid or missing.");
        }
    }
}
