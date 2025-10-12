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

        // GET: /Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new CategoryCreateRequest());
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateRequest model)
        {
            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/categories", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tạo danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(
                    string.Empty,
                    "Lỗi khi tạo danh mục. Tên có thể đã tồn tại."
                );
            }
            return View(model);
        }

        // GET: /Admin/Categories/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var category = JsonSerializer.Deserialize<CategoryViewModel>(
                    jsonString,
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
                return View(model);
            }
            return NotFound();
        }

        // POST: /Admin/Categories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryUpdateRequest model)
        {
            if (id != model.CategoryId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(model),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await client.PutAsync("api/categories", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật danh mục.");
            }
            return View(model);
        }

        // GET: /Admin/Categories/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var category = JsonSerializer.Deserialize<CategoryViewModel>(
                    jsonString,
                    _jsonOptions
                );
                return View(category);
            }
            return NotFound();
        }

        // POST: /Admin/Categories/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/categories/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }
            else // Sửa lại: chỉ cần else là đủ
            {
                TempData["ErrorMessage"] =
                    "Lỗi khi xóa danh mục. Có thể danh mục đang được sử dụng bởi sản phẩm.";
            }
            return RedirectToAction(nameof(Index));
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
