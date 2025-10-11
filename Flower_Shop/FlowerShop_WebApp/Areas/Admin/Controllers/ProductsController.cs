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

        public async Task<IActionResult> Index(string searchString)
        {
            var client = CreateApiClient();

            // Xây dựng URL động để bao gồm cả từ khóa tìm kiếm nếu có
            var apiUrl = "api/products?pageSize=100";
            if (!string.IsNullOrEmpty(searchString))
            {
                apiUrl += $"&search={searchString}";
            }

            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<ProductViewModel>
                >(_jsonOptions);

                // Gửi lại từ khóa tìm kiếm về view để hiển thị trong ô input
                ViewBag.CurrentFilter = searchString;

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

                // Sử dụng MultipartFormDataContent để gửi cả dữ liệu form và file
                using var formData = new MultipartFormDataContent();

                // Thêm các thuộc tính của model vào form data
                formData.Add(new StringContent(model.Name), "Name");
                formData.Add(new StringContent(model.Description ?? ""), "Description");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                formData.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
                formData.Add(new StringContent(model.StockQuantity.ToString()), "StockQuantity");
                formData.Add(new StringContent(model.IsActive.ToString()), "IsActive");

                // Thêm các file hình ảnh
                if (model.ImageFiles != null)
                {
                    foreach (var file in model.ImageFiles)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        formData.Add(streamContent, "ImageFiles", file.FileName);
                    }
                }

                // API endpoint cần được cập nhật để nhận multipart/form-data
                var response = await client.PostAsync("api/products", formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(
                    string.Empty,
                    "Error creating product. Please check API logs."
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
                    ExistingImageUrls = product.ImageUrls,
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
                using var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(model.ProductId.ToString()), "ProductId");
                formData.Add(new StringContent(model.Name), "Name");
                formData.Add(new StringContent(model.Description ?? ""), "Description");
                formData.Add(new StringContent(model.Price.ToString()), "Price");
                formData.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
                formData.Add(new StringContent(model.StockQuantity.ToString()), "StockQuantity");
                formData.Add(new StringContent(model.IsActive.ToString()), "IsActive");

                if (model.ImageFiles != null)
                {
                    foreach (var file in model.ImageFiles)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        formData.Add(streamContent, "ImageFiles", file.FileName);
                    }
                }

                // API endpoint cho Update cũng cần được cập nhật
                var response = await client.PutAsync($"api/products/{id}", formData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
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
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }
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
