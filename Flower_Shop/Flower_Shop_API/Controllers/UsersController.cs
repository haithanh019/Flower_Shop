using System.Security.Claims;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Users;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Toàn bộ controller này chỉ dành cho Admin
    public class UsersController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public UsersController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] QueryParameters queryParams)
        {
            var users = await _facadeService.UserService.GetAllUsersAsync(queryParams);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _facadeService.UserService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
        {
            var updatedUser = await _facadeService.UserService.UpdateUserAsync(request);
            return Ok(updatedUser);
        }

        [HttpGet("me")]
        [Authorize] // Cho phép người dùng đã đăng nhập lấy thông tin của chính mình
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            // Allow Admin to use this endpoint as well
            var user = await _facadeService.UserService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User profile not found.");
            }

            return Ok(user);
        }
    }
}
