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
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OrderService> _logger; // Thêm logger

        // Cập nhật hàm khởi tạo để nhận ILogger
        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailQueue emailQueue,
            IVietQRService vietQRService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<OrderService> logger // Thêm logger
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailQueue = emailQueue;
            _vietQRService = vietQRService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger; // Gán logger
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
                var oldStatus = orderToUpdate.Status;
                if (newStatus == OrderStatus.Confirmed && oldStatus != OrderStatus.Confirmed)
                {
                    // Chỉ gửi mail cho các trường hợp không phải VietQR ở đây
                    if (
                        orderToUpdate.User != null
                        && orderToUpdate.Payment?.Method != PaymentMethod.VietQR
                    )
                    {
                        var subject =
                            $"[FlowerShop] Đơn hàng #{orderToUpdate.OrderNumber} đã được xác nhận";
                        var htmlMessage = EmailTemplateService.OrderConfirmedEmail(
                            orderToUpdate,
                            orderToUpdate.User
                        );
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

        public async Task<bool> VerifyVietQRPaymentAsync(Guid orderId)
        {
            var order = await _unitOfWork.Order.GetAsync(o => o.OrderId == orderId, "Payment,User");

            // Nếu đơn hàng không tồn tại hoặc đã được xác nhận (hoặc thanh toán đã xong) thì không cần xử lý nữa
            if (
                order == null
                || order.Status == OrderStatus.Confirmed
                || order.Payment?.Status == PaymentStatus.Accepted
            )
            {
                // Trả về true để client biết rằng thanh toán đã hoàn tất và dừng polling.
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
                _logger.LogError(
                    "Failed to fetch transactions from VietQR API. Status: {StatusCode}",
                    response.StatusCode
                );
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonString);

            if (!doc.RootElement.TryGetProperty("data", out var transactions))
            {
                _logger.LogWarning("VietQR transaction response does not contain 'data' property.");
                return false;
            }

            foreach (var transaction in transactions.EnumerateArray())
            {
                var amount = transaction.GetProperty("amount").GetInt32();
                var description = transaction.GetProperty("description").GetString() ?? "";

                _logger.LogInformation(
                    "Checking transaction: Amount='{Amount}', Description='{Description}' for OrderNumber='{OrderNumber}'",
                    amount,
                    description,
                    order.OrderNumber
                );

                // --- SỬA LỖI LOGIC TẠI ĐÂY ---
                if (
                    amount == (int)order.TotalAmount
                    && description.Equals(order.OrderNumber, StringComparison.OrdinalIgnoreCase)
                )
                {
                    _logger.LogInformation(
                        "Payment match found for Order ID {OrderId}. Updating status.",
                        order.OrderId
                    );

                    // 1. Cập nhật trạng thái Order
                    order.Status = OrderStatus.Confirmed;

                    // 2. Cập nhật trạng thái Payment
                    if (order.Payment != null)
                    {
                        order.Payment.Status = PaymentStatus.Accepted;
                        order.Payment.PaidAt = DateTime.UtcNow;
                    }

                    // 3. Gửi email xác nhận thanh toán thành công
                    if (order.User != null)
                    {
                        var subject =
                            $"[FlowerShop] Thanh toán thành công cho đơn hàng #{order.OrderNumber}";
                        var htmlMessage = EmailTemplateService.PaymentSuccessEmail(
                            order,
                            order.User
                        );
                        _emailQueue.QueueEmail(order.User.Email, subject, htmlMessage);
                        _logger.LogInformation(
                            "Queued payment success email for Order ID {OrderId} to {Email}",
                            order.OrderId,
                            order.User.Email
                        );
                    }

                    // 4. Lưu tất cả thay đổi vào CSDL
                    await _unitOfWork.SaveAsync();

                    return true; // Tìm thấy, đã xử lý, trả về true
                }
            }
            return false; // Không tìm thấy giao dịch khớp
        }
    }
}
