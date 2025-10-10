using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FlowerShop_WebApp.Areas.Admin.Models.Products; // Using mới
using FlowerShop_WebApp.Models.Categories;
using FlowerShop_WebApp.Models.Products;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class ProductsController : BaseAdminController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductsController> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ProductsController(
            IHttpClientFactory httpClientFactory,
            ILogger<ProductsController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/products?pageSize=100");
            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<ProductViewModel>
                >(_jsonOptions);
                return View(pagedResult?.Items ?? new List<ProductViewModel>());
            }
            return View(new List<ProductViewModel>());
        }

        // GET: /Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            var model = new ProductCreateRequest
            {
                CategoryList = await GetCategorySelectListAsync(),
            };
            return View(model);
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateRequest model)
        {
            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var apiRequest = new
                {
                    model.Name,
                    model.Description,
                    model.Price,
                    model.CategoryId,
                    model.StockQuantity,
                    model.IsActive,
                    ImageUrls = new List<string>(), // Tạm thời để trống
                    ImagePublicIds = new List<string>(), // Tạm thời để trống
                };

                var response = await client.PostAsJsonAsync("api/products", apiRequest);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(
                    string.Empty,
                    "Error creating product. Please check the data."
                );
            }
            model.CategoryList = await GetCategorySelectListAsync();
            return View(model);
        }

        // GET: /Admin/Products/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<ProductViewModel>(
                    _jsonOptions
                );
                if (product == null)
                    return NotFound();

                var model = new ProductUpdateRequest
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    CategoryList = await GetCategorySelectListAsync(product.CategoryId),
                };
                return View(model);
            }
            return NotFound();
        }

        // POST: /Admin/Products/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductUpdateRequest model)
        {
            if (id != model.ProductId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var client = CreateApiClient();
                var apiRequest = new
                {
                    model.ProductId,
                    model.Name,
                    model.Description,
                    model.Price,
                    model.CategoryId,
                    model.StockQuantity,
                    model.IsActive,
                    ImageUrls = new List<string>(), // Tạm thời
                    ImagePublicIds = new List<string>(), // Tạm thời
                };

                var response = await client.PutAsJsonAsync("api/products", apiRequest);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Error updating product.");
            }
            model.CategoryList = await GetCategorySelectListAsync(model.CategoryId);
            return View(model);
        }

        // GET: /Admin/Products/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync($"api/products/{id}");
            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<ProductViewModel>(
                    _jsonOptions
                );
                return View(product);
            }
            return NotFound();
        }

        // POST: /Admin/Products/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/products/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Error deleting product.";
            }
            return RedirectToAction(nameof(Index));
        }

        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("ADMIN ACTION: Token is NULL or EMPTY in session!");
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }
            return client;
        }

        private async Task<SelectList> GetCategorySelectListAsync(object? selectedValue = null)
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/categories");
            if (response.IsSuccessStatusCode)
            {
                var categories = await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>(
                    _jsonOptions
                );
                return new SelectList(categories, "CategoryId", "Name", selectedValue);
            }
            return new SelectList(new List<CategoryViewModel>());
        }
    }
}
