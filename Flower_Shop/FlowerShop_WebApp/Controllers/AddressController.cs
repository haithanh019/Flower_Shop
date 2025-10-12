using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowerShop_WebApp.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _apiJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public AddressController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Address
        // Hiển thị danh sách địa chỉ
        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/address");

            if (response.IsSuccessStatusCode)
            {
                var addresses = await response.Content.ReadFromJsonAsync<List<AddressViewModel>>(
                    _apiJsonOptions
                );
                return View(addresses ?? new List<AddressViewModel>());
            }

            return View(new List<AddressViewModel>());
        }

        // GET: /Address/Create
        // Hiển thị form để thêm địa chỉ mới
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Address/Create
        // Xử lý việc thêm địa chỉ mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _apiJsonOptions),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PostAsync("api/address", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Thêm địa chỉ mới thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi thêm địa chỉ.");
            return View(model);
        }

        // GET: /Address/Edit/5
        // Hiển thị form để sửa địa chỉ
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = CreateApiClient();
            // Cần một endpoint API để lấy chi tiết một địa chỉ, tạm thời lấy tất cả và lọc ra
            var response = await client.GetAsync("api/address");
            if (response.IsSuccessStatusCode)
            {
                var addresses = await response.Content.ReadFromJsonAsync<List<AddressViewModel>>(
                    _apiJsonOptions
                );
                var addressToEdit = addresses?.FirstOrDefault(a => a.AddressId == id);
                if (addressToEdit != null)
                {
                    return View(addressToEdit);
                }
            }
            return NotFound();
        }

        // POST: /Address/Edit/5
        // Xử lý việc sửa địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AddressViewModel model)
        {
            if (id != model.AddressId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = CreateApiClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(model, _apiJsonOptions),
                Encoding.UTF8,
                "application/json"
            );
            var response = await client.PutAsync($"api/address/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật địa chỉ thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi cập nhật.");
            return View(model);
        }

        // POST: /Address/Delete/5
        // Xử lý việc xóa địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var client = CreateApiClient();
            var response = await client.DeleteAsync($"api/address/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xóa địa chỉ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Đã có lỗi xảy ra khi xóa địa chỉ.";
            }
            return RedirectToAction("Index");
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
    }
}
