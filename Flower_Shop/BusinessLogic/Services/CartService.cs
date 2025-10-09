using AutoMapper;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartDto> GetCartAsync(Guid? userId, string? sessionId)
        {
            Cart? cart;
            if (userId.HasValue)
            {
                cart = await GetOrCreateCartByUserId(userId.Value);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await GetOrCreateCartBySessionId(sessionId);
            }
            else
            {
                return new CartDto(); // Trả về giỏ hàng trống nếu không có thông tin định danh
            }

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> AddItemToCartAsync(Guid? userId, CartAddItemRequest request)
        {
            var cart = await GetCartForModification(userId, request.SessionId);

            var product = await _unitOfWork.Product.GetAsync(p =>
                p.ProductId == request.ProductId && p.IsActive
            );
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found or is inactive.");
            }
            if (product.StockQuantity < request.Quantity)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Quantity", new[] { "Not enough stock available." } },
                    }
                );
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
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
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> UpdateItemQuantityAsync(
            Guid? userId,
            CartUpdateQtyRequest request
        )
        {
            var cart = await GetCartForModification(userId, request.SessionId);
            var itemToUpdate = cart.Items.FirstOrDefault(i => i.CartItemId == request.CartItemId);

            if (itemToUpdate == null)
            {
                throw new KeyNotFoundException("Cart item not found.");
            }

            if (request.Quantity <= 0)
            {
                _unitOfWork.CartItem.Remove(itemToUpdate);
            }
            else
            {
                itemToUpdate.Quantity = request.Quantity;
            }

            await _unitOfWork.SaveAsync();
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> RemoveItemFromCartAsync(
            Guid? userId,
            CartRemoveItemRequest request
        )
        {
            var cart = await GetCartForModification(userId, request.SessionId);
            var itemToRemove = cart.Items.FirstOrDefault(i => i.CartItemId == request.CartItemId);

            if (itemToRemove != null)
            {
                _unitOfWork.CartItem.Remove(itemToRemove);
                await _unitOfWork.SaveAsync();
            }

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> MergeCartsAsync(Guid userId, CartMergeRequest request)
        {
            var userCart = await GetOrCreateCartByUserId(userId, "Items.Product");
            var guestCart = await _unitOfWork.Cart.GetAsync(
                c => c.SessionId == request.SessionId,
                "Items.Product"
            );

            if (guestCart == null || !guestCart.Items.Any())
            {
                return _mapper.Map<CartDto>(userCart);
            }

            foreach (var guestItem in guestCart.Items.ToList()) // Dùng ToList() để tránh lỗi khi thay đổi collection
            {
                var userItem = userCart.Items.FirstOrDefault(i =>
                    i.ProductId == guestItem.ProductId
                );
                if (userItem != null)
                {
                    userItem.Quantity += guestItem.Quantity;
                }
                else
                {
                    // Chuyển item từ giỏ hàng khách sang giỏ hàng user
                    guestItem.CartId = userCart.CartId;
                    userCart.Items.Add(guestItem);
                }
            }

            _unitOfWork.Cart.Remove(guestCart);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<CartDto>(userCart);
        }

        // --- Helper Methods ---
        private async Task<Cart> GetOrCreateCartByUserId(
            Guid userId,
            string includeProperties = "Items.Product.Images"
        )
        {
            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, includeProperties);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _unitOfWork.Cart.AddAsync(cart);
                // Không cần SaveAsync ở đây, sẽ save ở các hàm gọi nó
            }
            return cart;
        }

        private async Task<Cart> GetOrCreateCartBySessionId(
            string sessionId,
            string includeProperties = "Items.Product.Images"
        )
        {
            var cart = await _unitOfWork.Cart.GetAsync(
                c => c.SessionId == sessionId,
                includeProperties
            );
            if (cart == null)
            {
                cart = new Cart { SessionId = sessionId };
                await _unitOfWork.Cart.AddAsync(cart);
            }
            return cart;
        }

        private async Task<Cart> GetCartForModification(Guid? userId, string? sessionId)
        {
            Cart? cart;
            const string includeProps = "Items";

            if (userId.HasValue)
            {
                cart = await GetOrCreateCartByUserId(userId.Value, includeProps);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await GetOrCreateCartBySessionId(sessionId, includeProps);
            }
            else
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        {
                            "Identifier",
                            new[] { "Either User ID or Session ID is required to manage the cart." }
                        },
                    }
                );
            }
            return cart;
        }
    }
}
