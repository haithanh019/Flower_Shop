using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Users;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(Guid userId);

        Task<UserDto> UpdateUserAsync(UserUpdateRequest request);

        Task<PagedResultDto<UserDto>> GetAllUsersAsync(QueryParameters queryParams);
        Task<CustomerProfileDto?> GetCustomerProfileAsync(Guid userId);
        Task<CustomerProfileDto> UpdateCustomerProfileAsync(
            Guid userId,
            CustomerProfileUpdateRequest request
        );
        Task<bool> ChangePasswordAsync(Guid userId, CustomerPasswordChangeRequest request);
    }
}
