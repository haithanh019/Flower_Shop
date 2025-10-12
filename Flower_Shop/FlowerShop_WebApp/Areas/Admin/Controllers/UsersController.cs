using System.Net.Http.Headers;
using System.Text.Json;
using FlowerShop_WebApp.Models.Shared;
using FlowerShop_WebApp.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class UsersController : BaseAdminController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UsersController> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public UsersController(
            IHttpClientFactory httpClientFactory,
            ILogger<UsersController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string searchString)
        {
            var client = CreateApiClient();
            var apiUrl = "api/users?pageSize=100";
            if (!string.IsNullOrEmpty(searchString))
            {
                apiUrl += $"&search={searchString}";
            }

            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<UserViewModel>
                >(_jsonOptions);
                ViewBag.CurrentFilter = searchString;
                return View(pagedResult?.Items ?? new List<UserViewModel>());
            }

            return View(new List<UserViewModel>());
        }

        // GET: /Admin/Users/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserViewModel>(_jsonOptions);
                if (user == null)
                    return NotFound();

                ViewBag.Roles = new List<string> { "Khách hàng", "Quản trị viên" };

                return View(user);
            }

            return NotFound();
        }

        // POST: /Admin/Users/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserViewModel model)
        {
            if (id != model.UserId)
                return BadRequest();

            // === BẮT ĐẦU SỬA LỖI ===
            // Chuyển đổi giá trị tiếng Việt từ dropdown về giá trị Enum tiếng Anh trước khi gửi đi
            var roleToSend = model.Role == "Quản trị viên" ? "Admin" : "Customer";
            var updateRequest = new { model.UserId, Role = roleToSend };
            // === KẾT THÚC SỬA LỖI ===

            var client = CreateApiClient();
            var response = await client.PutAsJsonAsync("api/users", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật vai trò người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Failed to update user role for UserId {UserId}. API Response: {Response}",
                id,
                errorContent
            );
            TempData["ErrorMessage"] = "Lỗi khi cập nhật vai trò người dùng.";

            ViewBag.Roles = new List<string> { "Khách hàng", "Quản trị viên" };

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
