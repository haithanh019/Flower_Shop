using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Cart;
using FlowerShop_WebApp.Models.Orders;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    [Authorize] // Bắt buộc người dùng phải đăng nhập để truy cập controller này
    public class OrdersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public OrdersController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Action hiển thị trang Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var client = await CreateApiClientAsync();

            // Lấy thông tin giỏ hàng hiện tại của người dùng
            var response = await client.GetAsync("api/cart");

            if (!response.IsSuccessStatusCode)
            {
                // Nếu không lấy được giỏ hàng, có thể giỏ hàng trống, chuyển về trang giỏ hàng để hiển thị thông báo
                return RedirectToAction("Index", "Cart");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var cart = JsonSerializer.Deserialize<CartViewModel>(jsonString, _jsonOptions);

            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Cannot proceed to checkout.";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                Cart = cart,
                // TODO: Có thể lấy thông tin giao hàng mặc định từ profile người dùng ở đây
            };

            return View(checkoutViewModel);
        }

        // Action xử lý việc đặt hàng
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var client = await CreateApiClientAsync();

            // Lấy lại thông tin giỏ hàng để đảm bảo dữ liệu nhất quán
            var cartResponse = await client.GetAsync("api/cart");
            var cartJsonString = await cartResponse.Content.ReadAsStringAsync();
            var cart = JsonSerializer.Deserialize<CartViewModel>(cartJsonString, _jsonOptions);
            model.Cart = cart ?? new CartViewModel();

            if (!ModelState.IsValid)
            {
                return View("Checkout", model); // Nếu form không hợp lệ, trả về trang checkout với lỗi
            }

            // Tạo request body để gửi đến API
            var orderRequest = new
            {
                CustomerNote = model.CustomerNote,
                PaymentMethod = model.PaymentMethod,
                // Thông tin địa chỉ giao hàng sẽ được lấy từ profile user ở phía API
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(orderRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Gọi API để tạo đơn hàng
            var response = await client.PostAsync("api/orders", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Nếu thành công, chuyển hướng đến trang lịch sử đơn hàng
                // TODO: Có thể tạo một trang "Đặt hàng thành công" riêng
                return RedirectToAction("History");
            }
            else
            {
                // Nếu API trả về lỗi (ví dụ: hết hàng), hiển thị lỗi cho người dùng
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while placing your order. Please try again."
                );
                return View("Checkout", model);
            }
        }

        // TODO: Action History() để xem lịch sử đơn hàng
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var client = await CreateApiClientAsync();
            var response = await client.GetAsync("api/orders");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var pagedResult = JsonSerializer.Deserialize<PagedResultViewModel<OrderViewModel>>(
                    jsonString,
                    _jsonOptions
                );
                return View(pagedResult?.Items ?? new List<OrderViewModel>());
            }

            return View(new List<OrderViewModel>());
        }

        // Action hiển thị trang Chi tiết Đơn hàng
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var client = await CreateApiClientAsync();
            var response = await client.GetAsync($"api/orders/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<OrderViewModel>(jsonString, _jsonOptions);
                return View(order);
            }

            return NotFound();
        }

        // Helper method để tạo HttpClient đã đính kèm JWT Token
        private Task<HttpClient> CreateApiClientAsync()
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
            return Task.FromResult(client);
        }
    }
}
