using System.Text;
using System.Text.Json;
using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ultitity.Email.Interface;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailQueue _emailQueue;
        private readonly IVietQRService _vietQRService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        // Cập nhật hàm khởi tạo để nhận các dependency mới
        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailQueue emailQueue,
            IVietQRService vietQRService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailQueue = emailQueue;
            _vietQRService = vietQRService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // ... (Các phương thức GetAllOrdersAsync, CreateOrderFromCartAsync, GetOrderDetailsAsync, GetUserOrdersAsync không thay đổi)
        public async Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(QueryParameters queryParams)
        {
            var query = _unitOfWork.Order.GetQueryable("Items,Payment,User");
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

        public async Task<OrderDto> CreateOrderFromCartAsync(
            Guid userId,
            OrderCreateRequest request
        )
        {
            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, "Items.Product");
            if (cart == null || !cart.Items.Any())
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]> { { "Cart", new[] { "Your cart is empty." } } }
                );
            }

            var user = await _unitOfWork.User.GetAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

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
                    continue;
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

            var payment = new Payment
            {
                OrderId = newOrder.OrderId,
                Amount = newOrder.TotalAmount,
                Status = PaymentStatus.Pending,
                Method = Enum.Parse<PaymentMethod>(request.PaymentMethod, true),
            };
            await _unitOfWork.Payment.AddAsync(payment);

            _unitOfWork.CartItem.RemoveRange(cart.Items);
            await _unitOfWork.SaveAsync();

            var createdOrder = await _unitOfWork.Order.GetAsync(
                o => o.OrderId == newOrder.OrderId,
                includeProperties: "Payment"
            );

            if (createdOrder?.Payment?.Method == PaymentMethod.VietQR)
            {
                var qrDataURL = await _vietQRService.GenerateQRCode(createdOrder);
                if (!string.IsNullOrEmpty(qrDataURL) && createdOrder.Payment != null)
                {
                    createdOrder.Payment.TransactionId = qrDataURL;
                    await _unitOfWork.SaveAsync();
                }
            }

            return _mapper.Map<OrderDto>(createdOrder ?? newOrder);
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
            var orderToUpdate = await _unitOfWork.Order.GetAsync(
                o => o.OrderId == request.OrderId,
                "Items.Product,User,Payment"
            );
            if (orderToUpdate == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
            }

            if (Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                if (
                    newStatus == OrderStatus.Confirmed
                    && orderToUpdate.Status != OrderStatus.Confirmed
                )
                {
                    if (orderToUpdate.User != null)
                    {
                        string subject;
                        string htmlMessage;

                        if (orderToUpdate.Payment?.Method == PaymentMethod.VietQR)
                        {
                            subject =
                                $"[FlowerShop] Thanh toán thành công cho đơn hàng #{orderToUpdate.OrderNumber}";
                            htmlMessage = EmailTemplateService.PaymentSuccessEmail(
                                orderToUpdate,
                                orderToUpdate.User
                            );
                        }
                        else
                        {
                            subject =
                                $"[FlowerShop] Đơn hàng #{orderToUpdate.OrderNumber} đã được xác nhận";
                            htmlMessage = EmailTemplateService.OrderConfirmedEmail(
                                orderToUpdate,
                                orderToUpdate.User
                            );
                        }
                        _emailQueue.QueueEmail(orderToUpdate.User.Email, subject, htmlMessage);
                    }
                }
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

        // Phương thức mới để kiểm tra thanh toán VietQR
        public async Task<bool> VerifyVietQRPaymentAsync(Guid orderId)
        {
            var order = await _unitOfWork.Order.GetAsync(o => o.OrderId == orderId, "Payment,User");
            if (order == null || order.Status == OrderStatus.Confirmed)
            {
                return true;
            }

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["VietQR:ApiUrl"] + "transactions";
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("x-client-id", _configuration["VietQR:ClientId"]);
            request.Headers.Add("x-api-key", _configuration["VietQR:ApiKey"]);
            var payload = new
            {
                accountNo = _configuration["VietQR:AccountNumber"],
                acqId = _configuration["VietQR:BankBin"],
            };
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            if (!doc.RootElement.TryGetProperty("data", out var transactions))
            {
                return false;
            }

            foreach (var transaction in transactions.EnumerateArray())
            {
                var amount = transaction.GetProperty("amount").GetInt32();
                var description = transaction.GetProperty("description").GetString() ?? "";

                if (amount == (int)order.TotalAmount && description.Contains(order.OrderNumber))
                {
                    await UpdateOrderStatusAsync(
                        new OrderUpdateStatusRequest
                        {
                            OrderId = orderId,
                            Status = OrderStatus.Confirmed.ToString(),
                        }
                    );
                    return true;
                }
            }
            return false;
        }
    }
}
