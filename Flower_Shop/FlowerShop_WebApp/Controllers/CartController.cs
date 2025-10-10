using System.Net.Http.Headers;
using FlowerShop_WebApp.Models.ApiRequest;
using FlowerShop_WebApp.Models.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    [Authorize] // Bất kỳ ai muốn vào trang giỏ hàng đều phải đăng nhập
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CartController> _logger;

        public CartController(IHttpClientFactory httpClientFactory, ILogger<CartController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/cart");

            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartViewModel>();
                return View(cart ?? new CartViewModel());
            }

            return View(new CartViewModel());
        }

        [Authorize] // Thêm attribute ở đây để khi bấm nút, người dùng sẽ bị chuyển đến trang đăng nhập nếu chưa login
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            var client = CreateApiClient();
            var request = new CartAddItemRequest { ProductId = productId, Quantity = quantity };

            var response = await client.PostAsJsonAsync("api/cart/items", request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not add item to cart.";
            }

            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid cartItemId, int quantity)
        {
            var client = CreateApiClient();
            await client.PutAsJsonAsync("api/cart/items", new { cartItemId, quantity });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
        {
            var client = CreateApiClient();
            await client.PostAsJsonAsync("api/cart/items/remove", new { cartItemId });
            return RedirectToAction("Index");
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
}
