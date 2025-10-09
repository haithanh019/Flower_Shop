using System.Net.Http.Headers;
using System.Text.Json;
using FlowerShop_WebApp.Models.Products;
using FlowerShop_WebApp.Models.Shared;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Create()
        {
            return View();
        }

        // GET: /Admin/Products/Edit/5
        public IActionResult Edit(Guid id)
        {
            // Logic lấy sản phẩm và hiển thị form edit sẽ được thêm sau
            return View();
        }

        // GET: /Admin/Products/Delete/5
        public IActionResult Delete(Guid id)
        {
            // Logic lấy sản phẩm và hiển thị form xác nhận xóa sẽ được thêm sau
            return View();
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
