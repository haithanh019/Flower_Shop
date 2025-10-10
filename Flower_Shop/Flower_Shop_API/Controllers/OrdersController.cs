using System.Security.Claims;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Tất cả các hành động trong đây đều yêu cầu đăng nhập
    public class OrdersController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public OrdersController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet("all")] // Route mới cho admin
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền truy cập
        public async Task<IActionResult> GetAllOrders([FromQuery] QueryParameters queryParams)
        {
            var orders = await _facadeService.OrderService.GetAllOrdersAsync(queryParams);
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var userId = GetUserIdFromClaims();
            var order = await _facadeService.OrderService.CreateOrderFromCartAsync(userId, request);
            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders([FromQuery] QueryParameters queryParams)
        {
            var userId = GetUserIdFromClaims();
            var orders = await _facadeService.OrderService.GetUserOrdersAsync(userId, queryParams);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            var userId = GetUserIdFromClaims();
            var order = await _facadeService.OrderService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Đảm bảo người dùng chỉ có thể xem đơn hàng của chính họ, trừ khi là Admin
            if (order.CustomerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid(); // HTTP 403 Forbidden
            }

            return Ok(order);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được cập nhật trạng thái
        public async Task<IActionResult> UpdateOrderStatus(
            [FromBody] OrderUpdateStatusRequest request
        )
        {
            var updatedOrder = await _facadeService.OrderService.UpdateOrderStatusAsync(request);
            return Ok(updatedOrder);
        }

        [HttpPost("check-payment/{orderId}")]
        [AllowAnonymous] // Cho phép gọi mà không cần đăng nhập, vì webhook/polling không có token
        public async Task<IActionResult> CheckPaymentStatus(Guid orderId)
        {
            var result = await _facadeService.OrderService.VerifyVietQRPaymentAsync(orderId);
            if (result)
            {
                return Ok(new { success = true, message = "Payment confirmed." });
            }
            return Ok(new { success = false, message = "Payment not yet confirmed." });
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
