using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FlowerShop_WebApp.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IHttpClientFactory httpClientFactory,
            ILogger<DashboardController> logger
        )
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            var response = await client.GetAsync("api/dashboard/statistics");

            if (response.IsSuccessStatusCode)
            {
                var stats = await response.Content.ReadFromJsonAsync<DashboardViewModel>();
                if (stats != null)
                {
                    ViewBag.TotalRevenue = stats.TotalRevenue;
                    ViewBag.NewOrders = stats.NewOrders;
                    ViewBag.PendingOrders = stats.PendingOrders;
                    ViewBag.NewUsers = stats.NewUsers;
                }
            }
            else
            {
                // Đặt giá trị mặc định nếu gọi API thất bại
                ViewBag.TotalRevenue = 0;
                ViewBag.NewOrders = 0;
                ViewBag.PendingOrders = 0;
                ViewBag.NewUsers = 0;
                _logger.LogError(
                    "Failed to fetch dashboard statistics from API. Status code: {StatusCode}",
                    response.StatusCode
                );
            }

            return View();
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
