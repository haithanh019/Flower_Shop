// File: Flower_Shop_API/Controllers/OrdersController.cs

using System.IO; // Thêm using này
using System.Security.Claims;
using System.Text.Json; // Thêm using này
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Tạm thời comment dòng này để test webhook dễ hơn
    public class OrdersController : ControllerBase
    {
        private readonly IFacadeService _facadeService;
        private readonly ILogger<OrdersController> _logger; // <-- THÊM DÒNG NÀY

        // SỬA LẠI HÀM KHỞI TẠO (CONSTRUCTOR)
        public OrdersController(IFacadeService facadeService, ILogger<OrdersController> logger)
        {
            _facadeService = facadeService;
            _logger = logger; // <-- THÊM DÒNG NÀY
        }

        // ... (các phương thức GetAllOrders, CreateOrder, v.v. giữ nguyên) ...

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

        // PHƯƠNG THỨC WEBHOOK ĐỂ CHẨN ĐOÁN LỖI
        [HttpPost("payos-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOSWebhook()
        {
            _logger.LogInformation("--- WEBHOOK ENDPOINT HIT ---");
            try
            {
                string rawRequestBody = await new StreamReader(Request.Body).ReadToEndAsync();
                _logger.LogInformation("Raw Webhook Body: {RequestBody}", rawRequestBody);

                var webhookPayload = JsonSerializer.Deserialize<WebhookType>(rawRequestBody);

                if (webhookPayload != null)
                {
                    var data = _facadeService.PayOSService.VerifyPaymentWebhook(webhookPayload);
                    if (data != null)
                    {
                        await _facadeService.OrderService.HandlePayOSWebhook(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "--- ERROR PARSING OR PROCESSING WEBHOOK ---");
                return Ok("Error logged.");
            }
            return Ok("Webhook received.");
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
