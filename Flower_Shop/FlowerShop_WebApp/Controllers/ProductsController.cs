using System.Text.Json;
using FlowerShop_WebApp.Models.Categories;
using FlowerShop_WebApp.Models.Products;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    public class ProductsController : Controller
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

        public async Task<IActionResult> Index(int pageNumber = 1, Guid? categoryId = null)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Xây dựng URL cho API request, bao gồm cả phân trang và lọc
            var apiUrl = $"api/products?pageNumber={pageNumber}";
            if (categoryId.HasValue)
            {
                apiUrl += $"&categoryId={categoryId}";
            }

            // Lấy danh sách sản phẩm
            var productsResponse = await client.GetAsync(apiUrl);
            PagedResultViewModel<ProductViewModel> pagedProducts = new();
            if (productsResponse.IsSuccessStatusCode)
            {
                var jsonString = await productsResponse.Content.ReadAsStringAsync();
                pagedProducts =
                    JsonSerializer.Deserialize<PagedResultViewModel<ProductViewModel>>(
                        jsonString,
                        _jsonOptions
                    ) ?? pagedProducts;
            }

            // Lấy danh sách tất cả danh mục để hiển thị bộ lọc
            var categoriesResponse = await client.GetAsync("api/categories");
            List<CategoryViewModel> categories = new();
            if (categoriesResponse.IsSuccessStatusCode)
            {
                var jsonString = await categoriesResponse.Content.ReadAsStringAsync();
                categories =
                    JsonSerializer.Deserialize<List<CategoryViewModel>>(jsonString, _jsonOptions)
                    ?? categories;
            }

            // Dùng ViewBag để gửi danh sách categories và thông tin phân trang sang View
            ViewBag.Categories = categories;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)
                Math.Ceiling((double)pagedProducts.TotalCount / pagedProducts.PageSize);
            ViewBag.CurrentCategory = categoryId;

            return View(pagedProducts.Items);
        }
    }
}
