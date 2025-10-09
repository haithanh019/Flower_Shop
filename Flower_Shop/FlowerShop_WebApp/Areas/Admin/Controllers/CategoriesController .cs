using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

        public CategoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Index()
        {
            var client = await CreateApiClientAsync();
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
            return View();
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = await CreateApiClientAsync();
                var createRequest = new { model.Name, model.Description };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(createRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/categories", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(
                    string.Empty,
                    "Error creating category. The name might already exist."
                );
            }
            return View(model);
        }

        // GET: /Admin/Categories/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = await CreateApiClientAsync();
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

        // POST: /Admin/Categories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryViewModel model)
        {
            if (id != model.CategoryId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var client = await CreateApiClientAsync();
                var updateRequest = new
                {
                    model.CategoryId,
                    model.Name,
                    model.Description,
                };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(updateRequest),
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await client.PutAsync("api/categories", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error updating category.");
            }
            return View(model);
        }

        // GET: /Admin/Categories/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = await CreateApiClientAsync();
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
            var client = await CreateApiClientAsync();
            var response = await client.DeleteAsync($"api/categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Error deleting category. It might be in use by some products.";
            }
            return RedirectToAction(nameof(Index));
        }

        private Task<HttpClient> CreateApiClientAsync()
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
            return Task.FromResult(client);
        }
    }
}
