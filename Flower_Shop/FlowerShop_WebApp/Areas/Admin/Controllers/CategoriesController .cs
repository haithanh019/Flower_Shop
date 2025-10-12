using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Areas.Admin.Models.Categories;
using FlowerShop_WebApp.Models.Categories;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class CategoriesController : BaseAdminController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IHttpClientFactory httpClientFactory,
            ILogger<CategoriesController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/categories");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(
                    jsonString,
                    _jsonOptions
                );
                return View(categories);
            }
            return View(new List<CategoryViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> _CreateOrEditPartial(Guid? id)
        {
            if (id == null || id == Guid.Empty)
            {
                // Trường hợp tạo mới
                return PartialView("_CategoryFormPartial", new CategoryUpdateRequest());
            }

            // Trường hợp chỉnh sửa
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                var category = await response.Content.ReadFromJsonAsync<CategoryViewModel>(
                    _jsonOptions
                );
                if (category == null)
                    return NotFound();

                var model = new CategoryUpdateRequest
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                };
                return PartialView("_CategoryFormPartial", model);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryUpdateRequest model)
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

            var client = CreateApiClient();
            var response = await client.PostAsJsonAsync("api/categories", model);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Tạo danh mục thành công!" }
                );
            }

            return new BadRequestObjectResult(
                new
                {
                    success = false,
                    errors = new[] { "Lỗi khi tạo danh mục. Tên có thể đã tồn tại." },
                }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateRequest model)
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

            var client = CreateApiClient();
            var response = await client.PutAsJsonAsync("api/categories", model);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Cập nhật danh mục thành công!" }
                );
            }

            return new BadRequestObjectResult(
                new { success = false, errors = new[] { "Lỗi khi cập nhật danh mục." } }
            );
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Xóa danh mục thành công!" }
                );
            }

            return new BadRequestObjectResult(
                new { success = false, message = "Lỗi khi xóa. Danh mục có thể đang được sử dụng." }
            );
        }

        // --- PHƯƠNG THỨC HỖ TRỢ ---
        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("CreateApiClient: Token is NULL or EMPTY in session!");
            }
            else
            {
                _logger.LogInformation(
                    "CreateApiClient: Attaching token to request: {Token}",
                    token
                );
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }

            return client;
        }
    }
}
