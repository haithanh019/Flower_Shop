using System.Security.Claims;
using System.Text.Json; // Thêm using này
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
        private readonly ILogger<ProfileController> _logger; // Thêm ILogger

        public ProfileController(IFacadeService facadeService, ILogger<ProfileController> logger) // Cập nhật constructor
        {
            _facadeService = facadeService;
            _logger = logger; // Gán logger
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            _logger.LogInformation("--- [API] Received GET request for user profile.");
            var userId = GetUserIdFromClaims();
            _logger.LogInformation("--- [API] User ID from claims: {UserId}", userId);

            var profile = await _facadeService.UserService.GetCustomerProfileAsync(userId);

            if (profile == null)
            {
                _logger.LogWarning("--- [API] Profile not found for User ID: {UserId}", userId);
                return NotFound("Profile not found.");
            }

            _logger.LogInformation(
                "--- [API] Successfully retrieved profile for User ID: {UserId}",
                userId
            );
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] CustomerProfileUpdateRequest request
        )
        {
            _logger.LogInformation(
                "--- [API] Received PUT request to update profile. Data: {RequestData}",
                JsonSerializer.Serialize(request)
            );
            var userId = GetUserIdFromClaims();
            _logger.LogInformation("--- [API] User ID from claims: {UserId}", userId);

            var updatedProfile = await _facadeService.UserService.UpdateCustomerProfileAsync(
                userId,
                request
            );

            _logger.LogInformation(
                "--- [API] Successfully updated profile for User ID: {UserId}",
                userId
            );
            return Ok(updatedProfile);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] CustomerPasswordChangeRequest request
        )
        {
            _logger.LogInformation("--- [API] Received PUT request to change password.");
            var userId = GetUserIdFromClaims();
            _logger.LogInformation(
                "--- [API] User ID from claims for password change: {UserId}",
                userId
            );

            await _facadeService.UserService.ChangePasswordAsync(userId, request);

            _logger.LogInformation(
                "--- [API] Password changed successfully for User ID: {UserId}",
                userId
            );
            return Ok(new { message = "Password changed successfully." });
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            _logger.LogError("--- [API] CRITICAL: User ID claim is invalid or missing.");
            throw new UnauthorizedAccessException("User ID claim is invalid or missing.");
        }
    }
}
