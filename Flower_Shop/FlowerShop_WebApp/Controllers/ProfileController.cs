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
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
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
                // Thêm xử lý lỗi nếu cần
                return View(new CustomerProfileViewModel());
            }

            var profileJsonString = await profileResponse.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<CustomerProfileViewModel>(
                profileJsonString,
                _jsonOptions
            );

            var addressResponse = await client.GetAsync("api/address");
            if (addressResponse.IsSuccessStatusCode)
            {
                var addressJsonString = await addressResponse.Content.ReadAsStringAsync();
                var addresses = JsonSerializer.Deserialize<List<AddressViewModel>>(
                    addressJsonString,
                    _jsonOptions
                );
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
                return RedirectToAction("Index");
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync("api/profile", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
            }
            return RedirectToAction("Index");
        }

        // Action GET không đổi, chỉ dùng để lấy View rỗng ban đầu cho modal
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        // === BẮT ĐẦU THAY ĐỔI ===
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return new BadRequestObjectResult(new { success = false, errors });
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync("api/profile/change-password", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Password changed successfully!" }
                );
            }

            // Lấy lỗi từ API nếu có
            var errorContent = await response.Content.ReadAsStringAsync();
            // Cố gắng parse lỗi từ API (nếu API trả về cấu trúc lỗi chuẩn)
            // Trong trường hợp này, ta trả về một lỗi chung
            return new BadRequestObjectResult(
                new
                {
                    success = false,
                    errors = new[]
                    {
                        "Failed to change password. Please check your current password.",
                    },
                }
            );
        }

        // === KẾT THÚC THAY ĐỔI ===

        [HttpPost]
        public async Task<IActionResult> AddAddress(AddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/address", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Address added successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add address.";
                }
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
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PutAsync($"api/address/{model.AddressId}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Address updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update address.";
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(Guid addressId)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/address/{addressId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Address deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete address.";
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
    }
}
