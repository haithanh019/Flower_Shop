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
        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/users?pageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<UserViewModel>
                >(_jsonOptions);
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

                ViewBag.Roles = new List<string> { "Customer", "Admin" };
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

            // Chỉ cập nhật vai trò
            var updateRequest = new { model.UserId, model.Role };

            var client = CreateApiClient();
            var response = await client.PutAsJsonAsync("api/users", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError("Failed to update user role for UserId {UserId}", id);
            ModelState.AddModelError(string.Empty, "Error updating user role.");
            ViewBag.Roles = new List<string> { "Customer", "Admin" };
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
