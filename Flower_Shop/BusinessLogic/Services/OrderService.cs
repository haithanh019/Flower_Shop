using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ultitity.Email.Interface;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailQueue _emailQueue;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IEmailQueue emailQueue)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailQueue = emailQueue;
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
            // 1. Lấy giỏ hàng của người dùng
            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, "Items.Product");
            if (cart == null || !cart.Items.Any())
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]> { { "Cart", new[] { "Your cart is empty." } } }
                );
            }

            // 1.1 Lấy thông tin người dùng
            var user = await _unitOfWork.User.GetAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // 1.2 Lấy địa chỉ từ AddressId
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
            var fullShippingAddress =
                $"{address.Detail}, {address.Ward}, {address.District}, {address.City}";

            // 2. Tạo đối tượng Order và OrderItems
            var newOrder = new Order
            {
                UserId = userId,
                PhoneNumber = request.ShippingPhoneNumber,
                ShippingAddress = fullShippingAddress,
                Status = OrderStatus.Pending,
                OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            };

            decimal subtotal = 0;
            foreach (var cartItem in cart.Items)
            {
                if (cartItem.Product == null)
                {
                    continue; // Bỏ qua nếu sản phẩm không còn tồn tại
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

            // 5. Lưu tất cả thay đổi vào database
            await _unitOfWork.SaveAsync();

            // 6. Gửi email xác nhận sau khi đã lưu thành công
            var createdOrder = await _unitOfWork.Order.GetAsync(
                o => o.OrderId == newOrder.OrderId,
                includeProperties: "Items.Product,User" // Nạp lại đầy đủ thông tin
            );

            if (createdOrder != null && createdOrder.User != null)
            {
                var subject = $"[FlowerShop] Xác nhận đơn hàng #{createdOrder.OrderNumber}";
                var htmlMessage = EmailTemplateService.OrderConfirmationEmail(
                    createdOrder,
                    createdOrder.User
                );
                _emailQueue.QueueEmail(createdOrder.User.Email, subject, htmlMessage);
            }

            // 7. Trả về kết quả
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
