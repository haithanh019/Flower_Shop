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

        public async Task<IActionResult> Index(string statusFilter, int pageNumber = 1)
        {
            var client = CreateApiClient();
            var pageSize = 10;
            var apiUrl = $"api/orders/all?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(statusFilter))
            {
                apiUrl += $"&search={statusFilter}";
            }

            var response = await client.GetAsync(apiUrl);
            var pagedResult = new PagedResultViewModel<OrderViewModel>();

            if (response.IsSuccessStatusCode)
            {
                pagedResult =
                    await response.Content.ReadFromJsonAsync<PagedResultViewModel<OrderViewModel>>(
                        _jsonOptions
                    ) ?? new PagedResultViewModel<OrderViewModel>();
            }

            ViewBag.CurrentStatusFilter = statusFilter;

            var enumsResponse = await client.GetAsync("api/utility/enums");
            if (enumsResponse.IsSuccessStatusCode)
            {
                var enums = await enumsResponse.Content.ReadFromJsonAsync<AllEnumsResponse>(
                    _jsonOptions
                );
                ViewBag.OrderStatuses = enums?.OrderStatus ?? new List<EnumDto>();
            }
            else
            {
                ViewBag.OrderStatuses = new List<EnumDto>();
            }

            // Nếu là request AJAX thì chỉ trả về partial view của bảng
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OrderTablePartial", pagedResult);
            }

            return View(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsPartial(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/orders/{id}");

            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<OrderViewModel>(_jsonOptions);
                if (order == null)
                    return NotFound();

                var enumsResponse = await client.GetAsync("api/utility/enums");
                if (enumsResponse.IsSuccessStatusCode)
                {
                    var enums = await enumsResponse.Content.ReadFromJsonAsync<AllEnumsResponse>(
                        _jsonOptions
                    );
                    ViewBag.OrderStatuses = enums?.OrderStatus ?? new List<EnumDto>();
                }
                return PartialView("_OrderDetailsPartial", order);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid orderId, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return new BadRequestObjectResult(
                    new
                    {
                        success = false,
                        errors = new[] { "Vui lòng chọn một trạng thái hợp lệ." },
                    }
                );
            }

            var client = CreateApiClient();
            var updateRequest = new { OrderId = orderId, Status = status };
            var response = await client.PutAsJsonAsync("api/orders/status", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Cập nhật trạng thái thành công!" }
                );
            }

            _logger.LogError("Failed to update order status. OrderId: {OrderId}", orderId);
            return new BadRequestObjectResult(
                new { success = false, errors = new[] { "Cập nhật trạng thái thất bại." } }
            );
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
            return client;
        }
    }

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
