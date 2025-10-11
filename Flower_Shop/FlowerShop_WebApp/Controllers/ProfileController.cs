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
        private readonly ILogger<ProfileController> _logger;

        private readonly JsonSerializerOptions _apiJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ProfileController(
            IHttpClientFactory httpClientFactory,
            ILogger<ProfileController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // [HttpGet] - Action này xử lý khi bạn truy cập trang
        public async Task<IActionResult> Index()
        {
            var profile = await GetCurrentProfileForView();
            return View(profile);
        }

        // [HttpPost] - Action này xử lý khi bạn nhấn nút "Cập nhật thông tin"
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // [HttpPost] - Action này xử lý khi bạn nhấn nút "Thêm địa chỉ"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin địa chỉ không hợp lệ.";
                return RedirectToAction("Index");
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _apiJsonOptions),
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
            return RedirectToAction("Index");
        }

        // [HttpPost] - Action này xử lý khi bạn nhấn nút "Lưu thay đổi" trong modal sửa địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin địa chỉ không hợp lệ.";
                return RedirectToAction("Index");
            }

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
            return RedirectToAction("Index");
        }

        // [HttpPost] - Action này xử lý khi bạn nhấn nút "Xóa" địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(Guid addressId)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/address/{addressId}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa địa chỉ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa địa chỉ thất bại.";
            }
            return RedirectToAction("Index");
        }

        // [HttpPost] - Action này xử lý khi bạn nhấn nút "Lưu thay đổi" trong modal đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đổi mật khẩu không hợp lệ. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _apiJsonOptions),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PutAsync("api/profile/change-password", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại.";
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
            else
            {
                _logger.LogWarning("--- [WebApp] JWT Token is missing from session.");
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
