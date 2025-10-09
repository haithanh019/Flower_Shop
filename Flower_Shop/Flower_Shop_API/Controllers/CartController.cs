using System.Security.Claims;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public CartController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart([FromQuery] string? sessionId)
        {
            var userId = GetUserIdFromClaims();
            var cart = await _facadeService.CartService.GetCartAsync(userId, sessionId);
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartAddItemRequest request)
        {
            var userId = GetUserIdFromClaims();
            var cart = await _facadeService.CartService.AddItemToCartAsync(userId, request);
            return Ok(cart);
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateItemQuantity([FromBody] CartUpdateQtyRequest request)
        {
            var userId = GetUserIdFromClaims();
            var cart = await _facadeService.CartService.UpdateItemQuantityAsync(userId, request);
            return Ok(cart);
        }

        [HttpPost("items/remove")] // Sử dụng POST để body có thể chứa request object
        public async Task<IActionResult> RemoveItemFromCart(
            [FromBody] CartRemoveItemRequest request
        )
        {
            var userId = GetUserIdFromClaims();
            var cart = await _facadeService.CartService.RemoveItemFromCartAsync(userId, request);
            return Ok(cart);
        }

        [HttpPost("merge")]
        [Authorize] // Yêu cầu người dùng phải đăng nhập để hợp nhất giỏ hàng
        public async Task<IActionResult> MergeCarts([FromBody] CartMergeRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID claim is missing.");
            }

            var cart = await _facadeService.CartService.MergeCartsAsync(userId.Value, request);
            return Ok(cart);
        }

        private Guid? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : (Guid?)null;
        }
    }
}
