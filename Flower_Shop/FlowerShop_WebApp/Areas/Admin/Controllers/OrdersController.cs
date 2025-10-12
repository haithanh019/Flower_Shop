using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FlowerShop_WebApp.Models; // Ensure this using directive is present
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
        public async Task<IActionResult> Index(string statusFilter, int pageNumber = 1)
        {
            var client = CreateApiClient();
            var pageSize = 10; // Số đơn hàng trên mỗi trang

            var apiUrl = $"api/orders/all?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(statusFilter))
            {
                apiUrl += $"&search={statusFilter}";
            }

            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<OrderViewModel>
                >(_jsonOptions);

                // Truyền dữ liệu phân trang và bộ lọc về View
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = (int)
                    Math.Ceiling((pagedResult?.TotalCount ?? 0) / (double)pageSize);
                ViewBag.CurrentStatusFilter = statusFilter;

                // Lấy danh sách trạng thái cho dropdown
                var enumsResponse = await client.GetAsync("api/utility/enums");
                if (enumsResponse.IsSuccessStatusCode)
                {
                    var enums = await enumsResponse.Content.ReadFromJsonAsync<AllEnumsResponse>(
                        _jsonOptions
                    );
                    ViewBag.OrderStatuses =
                        enums?.OrderStatus.Select(e => e.Value).ToList() ?? new List<string>();
                }
                else
                {
                    ViewBag.OrderStatuses = new List<string>();
                }

                return View(pagedResult); // Truyền cả đối tượng pagedResult về View
            }

            _logger.LogError(
                "Failed to fetch orders from API. Status code: {StatusCode}",
                response.StatusCode
            );
            return View(new PagedResultViewModel<OrderViewModel>()); // Trả về model trống
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

                // Lấy danh sách các trạng thái đơn hàng từ API
                var enumsResponse = await client.GetAsync("api/utility/enums");
                if (enumsResponse.IsSuccessStatusCode)
                {
                    var enums = await enumsResponse.Content.ReadFromJsonAsync<AllEnumsResponse>(
                        _jsonOptions
                    );
                    ViewBag.OrderStatuses =
                        enums?.OrderStatus.Select(e => e.Value).ToList() ?? new List<string>();
                }
                else
                {
                    // Fallback to a hard-coded list if the API call fails
                    ViewBag.OrderStatuses = new List<string>
                    {
                        "Pending",
                        "Confirmed",
                        "Shipping",
                        "Completed",
                        "Cancelled",
                    };
                }

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
                TempData["ErrorMessage"] = "Vui lòng chọn một trạng thái hợp lệ.";
                return RedirectToAction("Details", new { id = orderId });
            }

            var client = CreateApiClient();
            var updateRequest = new { OrderId = orderId, Status = status };

            var response = await client.PutAsJsonAsync("api/orders/status", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update order status. Response: {Error}", errorContent);
                TempData["ErrorMessage"] = "Cập nhật trạng thái đơn hàng thất bại.";
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

    // You might need to add this class if it's not accessible
    // or referenced from another project
    public class AllEnumsResponse
    {
        public List<EnumDto> OrderStatus { get; set; } = new();
        public List<EnumDto> PaymentMethod { get; set; } = new();
        public List<EnumDto> PaymentStatus { get; set; } = new();
        public List<EnumDto> UserRole { get; set; } = new();
    }

    public class EnumDto
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
