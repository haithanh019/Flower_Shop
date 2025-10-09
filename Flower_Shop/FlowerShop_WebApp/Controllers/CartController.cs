using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Cart;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };
        private const string SessionIdCookieName = "FlowerShop.SessionId";

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Action hiển thị trang giỏ hàng
        public async Task<IActionResult> Index()
        {
            var client = await CreateApiClientAsync();
            var sessionId = GetSessionId();

            var response = await client.GetAsync($"api/cart?sessionId={sessionId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var cart = JsonSerializer.Deserialize<CartViewModel>(jsonString, _jsonOptions);
                return View(cart);
            }

            // Nếu có lỗi, hiển thị giỏ hàng trống
            return View(new CartViewModel());
        }

        // Action thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            var client = await CreateApiClientAsync();
            var sessionId = GetSessionId();

            var requestBody = new
            {
                productId,
                quantity,
                sessionId,
            };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            await client.PostAsync("api/cart/items", jsonContent);

            // Sau khi thêm, chuyển hướng người dùng đến trang giỏ hàng
            return RedirectToAction("Index");
        }

        // Action xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var client = await CreateApiClientAsync();
            var sessionId = GetSessionId();

            var requestBody = new { cartItemId, sessionId };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // API yêu cầu phương thức POST cho việc xóa item
            await client.PostAsJsonAsync("api/cart/items/remove", requestBody);

            return RedirectToAction("Index");
        }

        // Action cập nhật số lượng
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveFromCart(cartItemId);
            }

            var client = await CreateApiClientAsync();
            var sessionId = GetSessionId();

            var requestBody = new
            {
                cartItemId,
                quantity,
                sessionId,
            };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            await client.PutAsync("api/cart/items", jsonContent);

            return RedirectToAction("Index");
        }

        // --- CÁC PHƯƠNG THỨC HỖ TRỢ ---

        // Lấy hoặc tạo SessionId cho khách vãng lai
        private string GetSessionId()
        {
            // Thử lấy sessionId từ cookie
            if (Request.Cookies.TryGetValue(SessionIdCookieName, out var sessionId))
            {
                return sessionId ?? CreateSessionId();
            }
            // Nếu không có, tạo mới
            return CreateSessionId();
        }

        private string CreateSessionId()
        {
            var newSessionId = Guid.NewGuid().ToString();
            // Lưu sessionId vào cookie của trình duyệt trong 30 ngày
            Response.Cookies.Append(
                SessionIdCookieName,
                newSessionId,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true, // Tăng cường bảo mật
                    IsEssential = true,
                }
            );
            return newSessionId;
        }

        // Tạo HttpClient đã đính kèm JWT Token nếu người dùng đã đăng nhập
        private async Task<HttpClient> CreateApiClientAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

                // ** Quan trọng: Hợp nhất giỏ hàng của khách vào giỏ hàng của user sau khi đăng nhập **
                var sessionId = Request.Cookies[SessionIdCookieName];
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var mergeRequestBody = new { sessionId };
                    await client.PostAsJsonAsync("api/cart/merge", mergeRequestBody);
                    // Xóa cookie của khách sau khi đã hợp nhất
                    Response.Cookies.Delete(SessionIdCookieName);
                }
            }
            return client;
        }
    }
}
