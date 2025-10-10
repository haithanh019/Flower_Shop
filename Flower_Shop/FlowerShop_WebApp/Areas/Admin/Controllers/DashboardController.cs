using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        // GET: /Admin/Dashboard
        public IActionResult Index()
        {
            // Trong tương lai, bạn sẽ gọi API để lấy các số liệu thực tế này.
            // Hiện tại, chúng ta sẽ dùng dữ liệu giả lập.
            ViewBag.TotalRevenue = 12550000; // Giả sử tổng doanh thu
            ViewBag.NewOrders = 15; // Giả sử có 15 đơn hàng mới
            ViewBag.PendingOrders = 4; // Giả sử có 4 đơn hàng đang chờ xử lý
            ViewBag.NewUsers = 5; // Giả sử có 5 người dùng mới

            return View();
        }
    }
}
