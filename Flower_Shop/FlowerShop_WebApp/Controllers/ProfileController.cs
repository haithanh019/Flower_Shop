using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    [Authorize] // Chỉ cho phép user đã đăng nhập
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
            var response = await client.GetAsync("api/profile");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var profile = JsonSerializer.Deserialize<CustomerProfileViewModel>(
                    jsonString,
                    _jsonOptions
                );
                return View(profile);
            }

            return View(new CustomerProfileViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(CustomerProfileUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
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
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to update profile.";
            return View("Index", model);
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] =
                "Failed to change password. Please check your current password.";
            return View(model);
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
