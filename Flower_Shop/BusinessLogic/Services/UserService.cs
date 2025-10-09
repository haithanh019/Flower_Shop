using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Users;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.UserId == userId);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<PagedResultDto<UserDto>> GetAllUsersAsync(QueryParameters queryParams)
        {
            var query = _unitOfWork.User.GetQueryable();

            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                query = query.Where(u =>
                    u.FullName.Contains(queryParams.Search) || u.Email.Contains(queryParams.Search)
                );
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<UserDto>
            {
                Items = _mapper.Map<IEnumerable<UserDto>>(users),
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize,
            };
        }

        public async Task<UserDto> UpdateUserAsync(UserUpdateRequest request)
        {
            var userToUpdate = await _unitOfWork.User.GetAsync(u => u.UserId == request.UserId);
            if (userToUpdate == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
            }

            // Dùng AutoMapper để cập nhật các trường được phép
            _mapper.Map(request, userToUpdate);

            // Xử lý thay đổi Role nếu có
            if (
                !string.IsNullOrEmpty(request.Role)
                && Enum.TryParse<UserRole>(request.Role, true, out var newRole)
            )
            {
                userToUpdate.Role = newRole;
            }

            await _unitOfWork.SaveAsync();

            return _mapper.Map<UserDto>(userToUpdate);
        }
    }
}
