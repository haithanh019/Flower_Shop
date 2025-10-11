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
    [Authorize]
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
                TempData["ErrorMessage"] =
                    "Giỏ hàng của bạn trống. Không thể tiến hành thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var profileResponse = await client.GetAsync("api/profile");
            var userProfile = new CustomerProfileViewModel();
            if (profileResponse.IsSuccessStatusCode)
            {
                userProfile =
                    await profileResponse.Content.ReadFromJsonAsync<CustomerProfileViewModel>(
                        _jsonOptions
                    );
            }

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

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var client = await CreateApiClientAsync();

            if (model.SelectedAddressId == Guid.Empty)
            {
                ModelState.AddModelError("SelectedAddressId", "Vui lòng chọn địa chỉ giao hàng.");
            }

            if (!ModelState.IsValid)
            {
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

            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(orderRequest, serializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PostAsync("api/orders", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var createdOrder = JsonSerializer.Deserialize<OrderViewModel>(
                    jsonString,
                    _jsonOptions
                );
                if (
                    model.PaymentMethod.Equals("PayOS", StringComparison.OrdinalIgnoreCase)
                    && createdOrder != null
                    && !string.IsNullOrEmpty(createdOrder.TransactionId)
                )
                {
                    return Redirect(createdOrder.TransactionId);
                }

                return RedirectToAction("History");
            }
            else
            {
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
                    "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại"
                );
                return View("Checkout", model);
            }
        }

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

        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("History");
        }

        [HttpGet]
        public IActionResult PaymentCancelled()
        {
            TempData["ErrorMessage"] = "Thanh toán đã bị hủy. Bạn có thể thử lại từ giỏ hàng.";
            return RedirectToAction("Index", "Cart");
        }

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
