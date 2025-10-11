using System.Diagnostics;
using System.Text.Json;
using FlowerShop_WebApp.Models;
using FlowerShop_WebApp.Models.Products;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // Tạo một client để gọi API
            var client = _httpClientFactory.CreateClient("ApiClient");

            // Gọi đến endpoint lấy sản phẩm của API
            var response = await client.GetAsync("api/products?PageSize=8");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                // Deserialize JSON trả về thành PagedResultViewModel chứa các ProductViewModel
                var pagedResult = JsonSerializer.Deserialize<
                    PagedResultViewModel<ProductViewModel>
                >(jsonString, _jsonOptions);

                // Gửi danh sách sản phẩm (Items) cho View "Index.cshtml"
                return View(pagedResult?.Items);
            }

            // Nếu gọi API thất bại, hiển thị trang chủ với danh sách rỗng
            return View(new List<ProductViewModel>());
        }

        public async Task<IActionResult> ProductDetail(Guid id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"api/products/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductViewModel>(
                    jsonString,
                    _jsonOptions
                );

                // Gửi đối tượng sản phẩm cho View "ProductDetail.cshtml"
                return View(product);
            }

            // Nếu không tìm thấy sản phẩm, trả về lỗi 404
            return NotFound();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
