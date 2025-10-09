using Microsoft.AspNetCore.Mvc;

namespace FlowerShop_WebApp.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        // GET: /Admin/Dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}
