using AutoMapper;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    // PHIÊN BẢN HOÀN THIỆN CUỐI CÙNG - SỬA LỖI CONCURRENCY
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartDto> GetCartAsync(Guid userId)
        {
            var cart = await _unitOfWork.Cart.GetAsync(
                c => c.UserId == userId,
                "Items.Product.Images"
            );
            return _mapper.Map<CartDto>(cart ?? new Cart { UserId = userId });
        }

        public async Task<CartDto> AddItemToCartAsync(Guid userId, CartAddItemRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var product = await _unitOfWork.Product.GetAsync(p =>
                p.ProductId == request.ProductId && p.IsActive
            );
            if (product == null)
                throw new KeyNotFoundException("Product not found or is inactive.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + request.Quantity;
                if (product.StockQuantity < newQuantity)
                    throw new CustomValidationException(
                        new Dictionary<string, string[]>
                        {
                            { "Quantity", new[] { "Not enough stock." } },
                        }
                    );
                existingItem.Quantity = newQuantity;
            }
            else
            {
                if (product.StockQuantity < request.Quantity)
                    throw new CustomValidationException(
                        new Dictionary<string, string[]>
                        {
                            { "Quantity", new[] { "Not enough stock." } },
                        }
                    );
                cart.Items.Add(
                    new CartItem
                    {
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        UnitPrice = product.Price,
                    }
                );
            }

            await _unitOfWork.SaveAsync();
            return await GetCartAsync(userId);
        }

        public async Task<CartDto> UpdateItemQuantityAsync(
            Guid userId,
            CartUpdateQtyRequest request
        )
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == request.CartItemId);
            if (item == null)
                throw new KeyNotFoundException("Item not found in cart.");

            if (request.Quantity <= 0)
            {
                _unitOfWork.CartItem.Remove(item);
            }
            else
            {
                item.Quantity = request.Quantity;
            }

            await _unitOfWork.SaveAsync();
            return await GetCartAsync(userId);
        }

        public async Task<CartDto> RemoveItemFromCartAsync(
            Guid userId,
            CartRemoveItemRequest request
        )
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == request.CartItemId);
            if (item != null)
            {
                _unitOfWork.CartItem.Remove(item);
                await _unitOfWork.SaveAsync();
            }
            return await GetCartAsync(userId);
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            // Luôn sử dụng AsTracking khi chúng ta có ý định sửa đổi dữ liệu
            var cart = await _unitOfWork
                .Cart.GetQueryable("Items")
                .AsTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _unitOfWork.Cart.AddAsync(cart);
                await _unitOfWork.SaveAsync();
            }
            return cart;
        }
    }
}
