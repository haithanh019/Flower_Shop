using System.Threading.Tasks;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền truy cập
    public class DashboardController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public DashboardController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var statistics = await _facadeService.DashboardService.GetDashboardStatisticsAsync();
            return Ok(statistics);
        }
    }
}
