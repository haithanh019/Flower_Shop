using BusinessLogic.DTOs.Address;

namespace BusinessLogic.Services.Interfaces
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(Guid userId);
        Task<AddressDto> AddAddressAsync(Guid userId, AddressCreateRequest request);
        Task<AddressDto> UpdateAddressAsync(Guid userId, AddressUpdateRequest request);
        Task DeleteAddressAsync(Guid userId, Guid addressId);
    }
}
