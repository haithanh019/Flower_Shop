using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Cart;
using FlowerShop_WebApp.Models.Orders;
using FlowerShop_WebApp.Models.Profile;
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
            var cartResponse = await client.GetAsync("api/cart");

            if (!cartResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Cart");
            }

            var cart = await cartResponse.Content.ReadFromJsonAsync<CartViewModel>(_jsonOptions);

            if (cart == null || !cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Cannot proceed to checkout.";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy thông tin profile để điền tên và SĐT
            var profileResponse = await client.GetAsync("api/profile");
            var userProfile = new CustomerProfileViewModel();
            if (profileResponse.IsSuccessStatusCode)
            {
                userProfile =
                    await profileResponse.Content.ReadFromJsonAsync<CustomerProfileViewModel>(
                        _jsonOptions
                    );
            }

            // Lấy danh sách địa chỉ đã lưu
            var addressResponse = await client.GetAsync("api/address");
            var savedAddresses = new List<AddressViewModel>();
            if (addressResponse.IsSuccessStatusCode)
            {
                savedAddresses = await addressResponse.Content.ReadFromJsonAsync<
                    List<AddressViewModel>
                >(_jsonOptions);
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                Cart = cart,
                ShippingFullName = userProfile?.FullName ?? "",
                ShippingPhoneNumber = userProfile?.PhoneNumber ?? "",
                SavedAddresses = savedAddresses ?? new List<AddressViewModel>(),
            };

            return View(checkoutViewModel);
        }

        // Action xử lý việc đặt hàng
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var client = await CreateApiClientAsync();

            if (model.SelectedAddressId == Guid.Empty)
            {
                ModelState.AddModelError("SelectedAddressId", "Please select a shipping address.");
            }

            if (!ModelState.IsValid)
            {
                // Nạp lại dữ liệu cần thiết nếu form không hợp lệ
                var cartResponse = await client.GetAsync("api/cart");
                model.Cart =
                    await cartResponse.Content.ReadFromJsonAsync<CartViewModel>(_jsonOptions)
                    ?? new CartViewModel();

                var addressResponse = await client.GetAsync("api/address");
                model.SavedAddresses =
                    await addressResponse.Content.ReadFromJsonAsync<List<AddressViewModel>>(
                        _jsonOptions
                    ) ?? new List<AddressViewModel>();

                return View("Checkout", model);
            }

            var orderRequest = new
            {
                model.CustomerNote,
                model.PaymentMethod,
                model.ShippingFullName,
                model.ShippingPhoneNumber,
                AddressId = model.SelectedAddressId,
            };

            // --- BẮT ĐẦU THAY ĐỔI ---
            // Cấu hình serializer để sử dụng camelCase
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(orderRequest, serializerOptions), // Áp dụng cấu hình tại đây
                Encoding.UTF8,
                "application/json"
            );
            // --- KẾT THÚC THAY ĐỔI ---

            var response = await client.PostAsync("api/orders", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("History");
            }
            else
            {
                // Nạp lại dữ liệu cho view nếu có lỗi từ API
                var cartResponse = await client.GetAsync("api/cart");
                model.Cart =
                    await cartResponse.Content.ReadFromJsonAsync<CartViewModel>(_jsonOptions)
                    ?? new CartViewModel();
                var addressResponse = await client.GetAsync("api/address");
                model.SavedAddresses =
                    await addressResponse.Content.ReadFromJsonAsync<List<AddressViewModel>>(
                        _jsonOptions
                    ) ?? new List<AddressViewModel>();

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
