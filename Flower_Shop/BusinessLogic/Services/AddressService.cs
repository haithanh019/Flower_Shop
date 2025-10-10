using AutoMapper;
using BusinessLogic.DTOs.Address;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;

namespace BusinessLogic.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByUserIdAsync(Guid userId)
        {
            var addresses = await _unitOfWork.Address.GetRangeAsync(a => a.UserId == userId);
            return _mapper.Map<IEnumerable<AddressDto>>(addresses);
        }

        public async Task<AddressDto> AddAddressAsync(Guid userId, AddressCreateRequest request)
        {
            var newAddress = _mapper.Map<Address>(request);
            newAddress.UserId = userId;

            await _unitOfWork.Address.AddAsync(newAddress);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<AddressDto>(newAddress);
        }

        public async Task<AddressDto> UpdateAddressAsync(Guid userId, AddressUpdateRequest request)
        {
            var addressToUpdate = await _unitOfWork.Address.GetAsync(a =>
                a.AddressId == request.AddressId && a.UserId == userId
            );
            if (addressToUpdate == null)
            {
                throw new KeyNotFoundException(
                    "Address not found or you don't have permission to edit it."
                );
            }

            _mapper.Map(request, addressToUpdate);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<AddressDto>(addressToUpdate);
        }

        public async Task DeleteAddressAsync(Guid userId, Guid addressId)
        {
            var addressToDelete = await _unitOfWork.Address.GetAsync(a =>
                a.AddressId == addressId && a.UserId == userId
            );
            if (addressToDelete == null)
            {
                throw new KeyNotFoundException(
                    "Address not found or you don't have permission to delete it."
                );
            }

            _unitOfWork.Address.Remove(addressToDelete);
            await _unitOfWork.SaveAsync();
        }
    }
}
