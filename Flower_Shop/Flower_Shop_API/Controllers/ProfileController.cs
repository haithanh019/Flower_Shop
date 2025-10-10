using System.Security.Claims;
using BusinessLogic.DTOs.Users;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public ProfileController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserIdFromClaims();
            var profile = await _facadeService.UserService.GetCustomerProfileAsync(userId);

            if (profile == null)
            {
                return NotFound("Profile not found.");
            }

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] CustomerProfileUpdateRequest request
        )
        {
            var userId = GetUserIdFromClaims();
            var updatedProfile = await _facadeService.UserService.UpdateCustomerProfileAsync(
                userId,
                request
            );
            return Ok(updatedProfile);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] CustomerPasswordChangeRequest request
        )
        {
            var userId = GetUserIdFromClaims();
            await _facadeService.UserService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Password changed successfully." });
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID claim is invalid or missing.");
        }
    }
}
