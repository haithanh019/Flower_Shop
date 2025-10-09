using BusinessLogic.DTOs.Auth;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public AuthController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _facadeService.AuthService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _facadeService.AuthService.LoginAsync(request);
            return Ok(result);
        }
    }
}
