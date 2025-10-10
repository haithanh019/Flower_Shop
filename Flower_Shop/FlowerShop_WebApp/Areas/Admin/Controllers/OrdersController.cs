using System.Net.Http.Headers;
using System.Text.Json;
using FlowerShop_WebApp.Models.Orders;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class OrdersController : BaseAdminController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrdersController> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public OrdersController(
            IHttpClientFactory httpClientFactory,
            ILogger<OrdersController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            // Lấy tất cả đơn hàng, có thể thêm phân trang sau
            var response = await client.GetAsync("api/orders?pageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<OrderViewModel>
                >(_jsonOptions);
                return View(pagedResult?.Items ?? new List<OrderViewModel>());
            }

            _logger.LogError(
                "Failed to fetch orders from API. Status code: {StatusCode}",
                response.StatusCode
            );
            return View(new List<OrderViewModel>());
        }

        // GET: /Admin/Orders/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/orders/{id}");

            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<OrderViewModel>(_jsonOptions);
                if (order == null)
                    return NotFound();

                // Lấy danh sách các trạng thái đơn hàng để hiển thị dropdown
                // Giả định bạn có một endpoint để lấy các Enums, nếu không, bạn có thể hard-code
                ViewBag.OrderStatuses = new List<string>
                {
                    "Pending",
                    "Confirmed",
                    "Shipping",
                    "Completed",
                    "Cancelled",
                };

                return View(order);
            }

            return NotFound();
        }

        // POST: /Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid orderId, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                TempData["ErrorMessage"] = "Please select a valid status.";
                return RedirectToAction("Details", new { id = orderId });
            }

            var client = CreateApiClient();
            var updateRequest = new { OrderId = orderId, Status = status };

            var response = await client.PutAsJsonAsync("api/orders/status", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Order status updated successfully!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update order status. Response: {Error}", errorContent);
                TempData["ErrorMessage"] = "Failed to update order status.";
            }

            return RedirectToAction("Details", new { id = orderId });
        }

        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }
            else
            {
                _logger.LogError("Admin action initiated but JWT token is missing from session.");
            }
            return client;
        }
    }
}
