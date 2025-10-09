using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public UtilityController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet("enums")]
        public IActionResult GetAllEnums()
        {
            var enums = _facadeService.UtilityService.GetAllEnums();
            return Ok(enums);
        }
    }
}
