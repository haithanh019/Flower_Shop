using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Areas.Admin.Models;
using FlowerShop_WebApp.Models.Categories;
using FlowerShop_WebApp.Models.Products;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ProductsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Admin/Products
        public async Task<IActionResult> Index()
        {
            var client = await CreateApiClientAsync();
            var response = await client.GetAsync("api/products?pageSize=100"); // Lấy nhiều sản phẩm để quản lý

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var pagedResult = JsonSerializer.Deserialize<
                    PagedResultViewModel<ProductViewModel>
                >(jsonString, _jsonOptions);
                return View(pagedResult?.Items);
            }
            return View(new List<ProductViewModel>());
        }

        // Tạm thời để trống các action Create, Edit, Delete.
        // Chúng ta sẽ tạo View trước rồi quay lại hoàn thiện logic sau.

        // GET: /Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            var model = new ProductEditViewModel
            {
                CategoryList = await GetCategorySelectListAsync(),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = await CreateApiClientAsync();
                // Lưu ý: ImageUrls và ImagePublicIds đang để trống vì chưa làm chức năng upload ảnh
                var createRequest = new
                {
                    model.Name,
                    model.Description,
                    model.Price,
                    model.CategoryId,
                    model.StockQuantity,
                    model.IsActive,
                    ImageUrls = new List<string>(),
                    ImagePublicIds = new List<string>(),
                };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(createRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("api/products", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error creating product.");
            }

            model.CategoryList = await GetCategorySelectListAsync();
            return View(model);
        }

        // GET: /Admin/Products/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = await CreateApiClientAsync();
            var response = await client.GetAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductViewModel>(
                    jsonString,
                    _jsonOptions
                );
                if (product == null)
                {
                    return NotFound();
                }
                var model = new ProductEditViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    CategoryList = await GetCategorySelectListAsync(),
                };
                return View(model);
            }
            return NotFound();
        }

        // POST: /Admin/Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductEditViewModel model)
        {
            if (id != model.ProductId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var client = await CreateApiClientAsync();
                // Tương tự Create, ImageUrls tạm để trống
                var updateRequest = new
                {
                    model.ProductId,
                    model.Name,
                    model.Description,
                    model.Price,
                    model.CategoryId,
                    model.StockQuantity,
                    model.IsActive,
                    ImageUrls = new List<string>(),
                    ImagePublicIds = new List<string>(),
                };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(updateRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PutAsync("api/products", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error updating product.");
            }

            model.CategoryList = await GetCategorySelectListAsync();
            return View(model);
        }

        // GET: /Admin/Products/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = await CreateApiClientAsync();
            var response = await client.GetAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductViewModel>(
                    jsonString,
                    _jsonOptions
                );
                return View(product);
            }
            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = await CreateApiClientAsync();
            var response = await client.DeleteAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            // Có thể thêm TempData để hiển thị lỗi ở trang Index
            TempData["ErrorMessage"] = "Error deleting product.";
            return RedirectToAction(nameof(Index));
        }

        // === Helper Methods ===
        private async Task<SelectList> GetCategorySelectListAsync()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var categoriesResponse = await client.GetAsync("api/categories");
            if (categoriesResponse.IsSuccessStatusCode)
            {
                var jsonString = await categoriesResponse.Content.ReadAsStringAsync();
                var categories =
                    JsonSerializer.Deserialize<List<CategoryViewModel>>(jsonString, _jsonOptions)
                    ?? new List<CategoryViewModel>();
                return new SelectList(categories, "CategoryId", "Name");
            }
            return new SelectList(new List<CategoryViewModel>());
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
