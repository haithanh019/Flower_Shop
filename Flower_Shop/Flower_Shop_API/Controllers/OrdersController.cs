// File: Flower_Shop_API/Controllers/OrdersController.cs

using System.IO; // Thêm using này
using System.Security.Claims;
using System.Text.Json; // Thêm using này
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IFacadeService _facadeService;
        private readonly ILogger<OrdersController> _logger;

        // SỬA LẠI HÀM KHỞI TẠO (CONSTRUCTOR)
        public OrdersController(IFacadeService facadeService, ILogger<OrdersController> logger)
        {
            _facadeService = facadeService;
            _logger = logger;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] QueryParameters queryParams)
        {
            var orders = await _facadeService.OrderService.GetAllOrdersAsync(queryParams);
            return Ok(orders);
        }

        [HttpPost]
        [Authorize] // Giữ Authorize cho các action cần thiết
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var userId = GetUserIdFromClaims();
            var order = await _facadeService.OrderService.CreateOrderFromCartAsync(userId, request);
            return Ok(order);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserOrders([FromQuery] QueryParameters queryParams)
        {
            var userId = GetUserIdFromClaims();
            var orders = await _facadeService.OrderService.GetUserOrdersAsync(userId, queryParams);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            var userId = GetUserIdFromClaims();
            var order = await _facadeService.OrderService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.CustomerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return Ok(order);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            [FromBody] OrderUpdateStatusRequest request
        )
        {
            var updatedOrder = await _facadeService.OrderService.UpdateOrderStatusAsync(request);
            return Ok(updatedOrder);
        }

        [HttpPost("payos-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook([FromBody] WebhookType webhookPayload)
        {
            _logger.LogInformation("--- PayOS Webhook endpoint was hit ---");
            try
            {
                var webhookData = _facadeService.PayOSService.VerifyPaymentWebhook(webhookPayload);

                await _facadeService.OrderService.HandlePayOSWebhook(webhookData);

                return Ok("Webhook processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "--- ERROR: An unexpected error occurred while processing webhook ---"
                );
                return Ok("An error occurred but has been logged.");
            }
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
