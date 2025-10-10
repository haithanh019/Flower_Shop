using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Thêm phương thức này vào lớp OrderService
        public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(QueryParameters queryParams)
        {
            var query = _unitOfWork.Order.GetQueryable("Items,Payment,User");

            // Thêm logic lọc theo trạng thái (sử dụng thuộc tính Search)
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                if (Enum.TryParse<OrderStatus>(queryParams.Search, true, out var status))
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            query = query.OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<OrderDto>
            {
                Items = _mapper.Map<IEnumerable<OrderDto>>(orders),
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize,
            };
        }

        //...
        public async Task<OrderDto> CreateOrderFromCartAsync(
            Guid userId,
            OrderCreateRequest request
        )
        {
            // 1. Lấy giỏ hàng của người dùng (giữ nguyên)
            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, "Items.Product");
            if (cart == null || !cart.Items.Any())
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]> { { "Cart", new[] { "Your cart is empty." } } }
                );
            }

            // --- BẮT ĐẦU CẬP NHẬT LOGIC ---

            // 1.1 Lấy địa chỉ từ AddressId
            var address = await _unitOfWork.Address.GetAsync(a =>
                a.AddressId == request.AddressId && a.UserId == userId
            );
            if (address == null)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "AddressId", new[] { "Invalid or unauthorized address." } },
                    }
                );
            }
            // Tạo chuỗi địa chỉ đầy đủ từ đối tượng address
            var fullShippingAddress =
                $"{address.Detail}, {address.Ward}, {address.District}, {address.City}";

            // --- KẾT THÚC CẬP NHẬT LOGIC ---

            // 2. Tạo đối tượng Order và OrderItems
            var newOrder = new Order
            {
                UserId = userId,
                PhoneNumber = request.ShippingPhoneNumber,
                ShippingAddress = fullShippingAddress, // Sử dụng chuỗi địa chỉ vừa tạo
                Status = OrderStatus.Pending,
                OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            };

            decimal subtotal = 0;
            foreach (var cartItem in cart.Items)
            {
                if (cartItem.Product == null)
                {
                    throw new InvalidOperationException(
                        $"Product details missing for cart item {cartItem.CartItemId}."
                    );
                }
                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                {
                    throw new CustomValidationException(
                        new Dictionary<string, string[]>
                        {
                            {
                                cartItem.Product.Name,
                                new[]
                                {
                                    $"Not enough stock for {cartItem.Product.Name}. Available: {cartItem.Product.StockQuantity}",
                                }
                            },
                        }
                    );
                }

                cartItem.Product.StockQuantity -= cartItem.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    LineTotal = cartItem.Quantity * cartItem.UnitPrice,
                };
                newOrder.Items.Add(orderItem);
                subtotal += orderItem.LineTotal;
            }

            newOrder.Subtotal = subtotal;
            newOrder.TotalAmount = subtotal;

            await _unitOfWork.Order.AddAsync(newOrder);

            // 3. Tạo thanh toán (Payment) tương ứng
            var payment = new Payment
            {
                OrderId = newOrder.OrderId,
                Amount = newOrder.TotalAmount,
                Status = PaymentStatus.Pending,
                Method = Enum.Parse<PaymentMethod>(request.PaymentMethod, true),
            };
            await _unitOfWork.Payment.AddAsync(payment);

            // 4. Xóa các sản phẩm trong giỏ hàng
            _unitOfWork.CartItem.RemoveRange(cart.Items);

            // 5. Lưu tất cả thay đổi
            await _unitOfWork.SaveAsync();

            return _mapper.Map<OrderDto>(newOrder);
        }

        public async Task<OrderDto?> GetOrderDetailsAsync(Guid orderId)
        {
            var order = await _unitOfWork.Order.GetAsync(
                o => o.OrderId == orderId,
                "Items.Product,User,Payment"
            );
            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        public async Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(
            Guid userId,
            QueryParameters queryParams
        )
        {
            var query = _unitOfWork
                .Order.GetQueryable("Items,Payment")
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var orders = await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PagedResultDto<OrderDto>
            {
                Items = _mapper.Map<IEnumerable<OrderDto>>(orders),
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize,
            };
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(OrderUpdateStatusRequest request)
        {
            var orderToUpdate = await _unitOfWork.Order.GetAsync(o => o.OrderId == request.OrderId);
            if (orderToUpdate == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
            }

            if (Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                orderToUpdate.Status = newStatus;
            }
            else
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Status", new[] { $"Invalid order status: {request.Status}." } },
                    }
                );
            }

            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDto>(orderToUpdate);
        }
    }
}
