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

        public async Task<IActionResult> Index(string searchString)
        {
            var client = CreateApiClient();
            var apiUrl = "api/users?pageSize=100";
            if (!string.IsNullOrEmpty(searchString))
            {
                apiUrl += $"&search={searchString}";
            }

            var response = await client.GetAsync(apiUrl);
            var users = new List<UserViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<UserViewModel>
                >(_jsonOptions);
                users = pagedResult?.Items.ToList() ?? new List<UserViewModel>();
            }
            ViewBag.CurrentFilter = searchString;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> _EditPartial(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserViewModel>(_jsonOptions);
                if (user == null)
                    return NotFound();

                ViewBag.Roles = new List<string> { "Khách hàng", "Quản trị viên" };
                return PartialView("_UserFormPartial", user);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(
                    new
                    {
                        success = false,
                        errors = ModelState
                            .Values.SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage),
                    }
                );
            }

            var roleToSend = model.Role == "Quản trị viên" ? "Admin" : "Customer";
            var updateRequest = new { model.UserId, Role = roleToSend };

            var client = CreateApiClient();
            var response = await client.PutAsJsonAsync("api/users", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Cập nhật vai trò thành công!" }
                );
            }

            _logger.LogError("Failed to update user role for UserId {UserId}", model.UserId);
            return new BadRequestObjectResult(
                new { success = false, errors = new[] { "Lỗi khi cập nhật vai trò." } }
            );
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
