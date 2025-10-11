using System.Security.Claims;
using BusinessLogic.DTOs.Address;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập cho tất cả
    public class AddressController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public AddressController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAddresses()
        {
            var userId = GetUserIdFromClaims();
            var addresses = await _facadeService.AddressService.GetAddressesByUserIdAsync(userId);
            return Ok(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] AddressCreateRequest request)
        {
            var userId = GetUserIdFromClaims();
            var newAddress = await _facadeService.AddressService.AddAddressAsync(userId, request);
            return Ok(newAddress);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(
            Guid id,
            [FromBody] AddressUpdateRequest request
        )
        {
            if (id != request.AddressId)
            {
                return BadRequest("Address ID mismatch.");
            }
            var userId = GetUserIdFromClaims();
            var updatedAddress = await _facadeService.AddressService.UpdateAddressAsync(
                userId,
                request
            );
            return Ok(updatedAddress);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var userId = GetUserIdFromClaims();
            await _facadeService.AddressService.DeleteAddressAsync(userId, id);
            return NoContent();
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
