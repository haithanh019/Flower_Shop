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

        // GET: /Profile/Index
        public async Task<IActionResult> Index()
        {
            var profile = await GetCurrentProfileForView();
            return View(profile);
        }

        // POST: /Profile/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CustomerProfileUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var profile = await GetCurrentProfileForView();
                profile.FullName = model.FullName;
                profile.PhoneNumber = model.PhoneNumber;
                return View(profile);
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

        // --- BỔ SUNG ACTIONS CHO TRANG ĐỔI MẬT KHẨU ---

        // GET: /Profile/ChangePassword
        // Hiển thị trang đổi mật khẩu
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Profile/ChangePassword
        // Xử lý việc đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(
                string.Empty,
                "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại."
            );
            return View(model);
        }

        // --- CÁC PHƯƠNG THỨC HỖ TRỢ ---

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
            var profileResponse = await client.GetAsync("api/profile");

            if (profileResponse.IsSuccessStatusCode)
            {
                var profile =
                    await profileResponse.Content.ReadFromJsonAsync<CustomerProfileViewModel>(
                        _apiJsonOptions
                    );
                return profile ?? new CustomerProfileViewModel();
            }

            return new CustomerProfileViewModel();
        }
    }
}
