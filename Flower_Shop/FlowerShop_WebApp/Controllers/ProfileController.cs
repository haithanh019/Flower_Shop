using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _apiJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var profileResponse = await client.GetAsync("api/profile");

            if (!profileResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể tải thông tin tài khoản.";
                return View(new CustomerProfileViewModel());
            }

            var profile = await profileResponse.Content.ReadFromJsonAsync<CustomerProfileViewModel>(
                _apiJsonOptions
            );

            var addressResponse = await client.GetAsync("api/address");
            if (addressResponse.IsSuccessStatusCode)
            {
                var addresses = await addressResponse.Content.ReadFromJsonAsync<
                    List<AddressViewModel>
                >(_apiJsonOptions);
                if (profile != null)
                {
                    profile.Addresses = addresses ?? new List<AddressViewModel>();
                }
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(CustomerProfileUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var profile = await GetCurrentProfileForView();
                profile.FullName = model.FullName;
                profile.PhoneNumber = model.PhoneNumber;
                return View("Index", profile);
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _apiJsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync("api/profile", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Cập nhật thông tin thất bại.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(AddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var addRequest = new
                {
                    model.City,
                    model.District,
                    model.Ward,
                    model.Detail,
                };

                // SỬA LỖI: Sử dụng _apiJsonOptions khi serialize
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(addRequest, _apiJsonOptions),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/address", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm địa chỉ thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Thêm địa chỉ thất bại.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin địa chỉ không hợp lệ.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditAddress(AddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model, _apiJsonOptions),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PutAsync($"api/address/{model.AddressId}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật địa chỉ thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cập nhật địa chỉ thất bại.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin địa chỉ không hợp lệ.";
            }
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

        private async Task<CustomerProfileViewModel> GetCurrentProfileForView()
        {
            var client = CreateApiClient();
            var profile = new CustomerProfileViewModel();

            var profileResponse = await client.GetAsync("api/profile");
            if (profileResponse.IsSuccessStatusCode)
            {
                profile = await profileResponse.Content.ReadFromJsonAsync<CustomerProfileViewModel>(
                    _apiJsonOptions
                );
            }

            var addressResponse = await client.GetAsync("api/address");
            if (addressResponse.IsSuccessStatusCode && profile != null)
            {
                profile.Addresses =
                    await addressResponse.Content.ReadFromJsonAsync<List<AddressViewModel>>(
                        _apiJsonOptions
                    ) ?? new List<AddressViewModel>();
            }

            return profile ?? new CustomerProfileViewModel();
        }
    }
}
