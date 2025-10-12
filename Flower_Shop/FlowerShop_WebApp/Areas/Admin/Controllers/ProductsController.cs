using System.Net.Http.Headers;
using System.Text.Json;
using FlowerShop_WebApp.Areas.Admin.Models.Products;
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
            var apiUrl = "api/products?pageSize=100&filterBool=false";
            if (!string.IsNullOrEmpty(searchString))
            {
                apiUrl += $"&search={searchString}";
            }
            var response = await client.GetAsync(apiUrl);
            var products = new List<ProductViewModel>();
            if (response.IsSuccessStatusCode)
            {
                var pagedResult = await response.Content.ReadFromJsonAsync<
                    PagedResultViewModel<ProductViewModel>
                >(_jsonOptions);
                products = pagedResult?.Items.ToList() ?? new List<ProductViewModel>();
            }
            ViewBag.CurrentFilter = searchString;
            return View(products);
        }

        // === BẮT ĐẦU THAY ĐỔI & THÊM MỚI ===

        [HttpGet]
        public async Task<IActionResult> _CreateOrEditPartial(Guid? id)
        {
            if (id == null || id == Guid.Empty)
            {
                var createModel = new ProductUpdateRequest
                {
                    CategoryList = await GetCategorySelectListAsync(),
                };
                return PartialView("_ProductFormPartial", createModel);
            }

            var client = CreateApiClient();
            var response = await client.GetAsync($"api/products/admin/{id}");
            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<ProductViewModel>(
                    _jsonOptions
                );
                if (product == null)
                    return NotFound();

                var updateModel = new ProductUpdateRequest
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
                return PartialView("_ProductFormPartial", updateModel);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest model)
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
            using var formData = BuildMultipartFormData(model);
            var response = await client.PostAsync("api/products", formData);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Tạo sản phẩm thành công!" }
                );
            }
            return new BadRequestObjectResult(
                new { success = false, errors = new[] { "Lỗi khi tạo sản phẩm." } }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ProductUpdateRequest model)
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
            using var formData = BuildMultipartFormData(model);
            var response = await client.PutAsync($"api/products/{model.ProductId}", formData);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Cập nhật sản phẩm thành công!" }
                );
            }
            return new BadRequestObjectResult(
                new { success = false, errors = new[] { "Lỗi khi cập nhật sản phẩm." } }
            );
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/products/{id}");
            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(
                    new { success = true, message = "Xóa sản phẩm thành công!" }
                );
            }
            return new BadRequestObjectResult(
                new { success = false, message = "Lỗi khi xóa sản phẩm." }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(Guid productId, string imageUrl)
        {
            if (productId == Guid.Empty || string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { success = false, message = "Thông tin không hợp lệ." });
            }

            var client = CreateApiClient();
            var request = new ProductImageDeleteRequest
            {
                ProductId = productId,
                ImageUrl = imageUrl,
            };

            var response = await client.PostAsJsonAsync("api/products/delete-image", request);

            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult(new { success = true, message = "Xóa ảnh thành công!" });
            }

            return new BadRequestObjectResult(
                new { success = false, message = "Lỗi khi xóa ảnh." }
            );
        }

        // Helper để tạo FormData
        private MultipartFormDataContent BuildMultipartFormData(object model)
        {
            var formData = new MultipartFormDataContent();
            var properties = model.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(model);
                if (value == null)
                    continue;

                if (value is List<IFormFile> files)
                {
                    foreach (var file in files)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        formData.Add(streamContent, prop.Name, file.FileName);
                    }
                }
                else if (
                    prop.PropertyType != typeof(SelectList)
                    && prop.PropertyType != typeof(ICollection<string>)
                )
                {
                    formData.Add(new StringContent(value.ToString() ?? ""), prop.Name);
                }
            }
            return formData;
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
