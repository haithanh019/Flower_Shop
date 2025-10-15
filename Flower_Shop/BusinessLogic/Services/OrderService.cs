using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.Events;
using BusinessLogic.Services.Interfaces;
using CloudinaryDotNet.Actions;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS.Types;
using Ultitity.Email.Interface;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailQueue _emailQueue;
        private readonly IPayOSService _payOSService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IEventPublisher _eventPublisher;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailQueue emailQueue,
            IPayOSService payOSService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IEventPublisher eventPublisher
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailQueue = emailQueue;
            _payOSService = payOSService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _eventPublisher = eventPublisher;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(
            Guid userId,
            OrderCreateRequest request
        )
        {
            var cart = await _unitOfWork.Cart.GetAsync(c => c.UserId == userId, "   Items.Product");
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
                includeProperties: "Payment,Items.Product,User"
            );
            if (
                createdOrder?.Payment?.Method == PaymentMethod.CashOnDelivery
                && createdOrder.User != null
            )
            {
                var subject = $"[FlowerShop] Đã tiếp nhận đơn hàng #{createdOrder.OrderNumber}";
                var htmlMessage = EmailTemplateService.OrderReceivedEmail(
                    createdOrder,
                    createdOrder.User
                );
                _emailQueue.QueueEmail(createdOrder.User.Email, subject, htmlMessage);
            }
            else if (createdOrder?.Payment?.Method == PaymentMethod.PayOS)
            {
                var paymentResult = await _payOSService.CreatePaymentLink(createdOrder);
                if (paymentResult != null && createdOrder.Payment != null)
                {
                    createdOrder.Payment.TransactionId = paymentResult.checkoutUrl;
                    await _unitOfWork.SaveAsync();
                }
            }
            if (createdOrder?.Payment?.Method == PaymentMethod.CashOnDelivery)
            {
                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderNumber = createdOrder.OrderNumber,
                    TotalAmount = createdOrder.TotalAmount,
                    CustomerName = request.ShippingFullName,
                };
                await _eventPublisher.PublishAsync(orderCreatedEvent);
            }

            return _mapper.Map<OrderDto>(createdOrder ?? newOrder);
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

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Status", new[] { $"Invalid order status: {request.Status}." } },
                    }
                );
            }

            var oldStatus = orderToUpdate.Status;
            orderToUpdate.Status = newStatus;
            if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                foreach (var item in orderToUpdate.Items)
                {
                    var product = await _unitOfWork.Product.GetAsync(p =>
                        p.ProductId == item.ProductId
                    );
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }

            if (newStatus != oldStatus && orderToUpdate.User != null)
            {
                string subject = string.Empty;
                string htmlMessage = string.Empty;

                switch (newStatus)
                {
                    case OrderStatus.Confirmed:
                        if (orderToUpdate.Payment?.Method == PaymentMethod.CashOnDelivery)
                        {
                            subject =
                                $"[FlowerShop] Đơn hàng #{orderToUpdate.OrderNumber} đã được xác nhận";
                            htmlMessage = EmailTemplateService.OrderConfirmedEmail(
                                orderToUpdate,
                                orderToUpdate.User
                            );
                        }
                        break;

                    case OrderStatus.Shipping:
                        subject =
                            $"[FlowerShop] Đơn hàng #{orderToUpdate.OrderNumber} đang được giao đến bạn";
                        htmlMessage = EmailTemplateService.OrderShippedEmail(
                            orderToUpdate,
                            orderToUpdate.User
                        );
                        break;

                    case OrderStatus.Completed:
                        subject =
                            $"[FlowerShop] Đơn hàng #{orderToUpdate.OrderNumber} đã được giao đến bạn";
                        htmlMessage = EmailTemplateService.OrderCompletedEmail(
                            orderToUpdate,
                            orderToUpdate.User
                        );
                        break;

                    case OrderStatus.Cancelled:
                        subject = $"[FlowerShop] Đã hủy đơn hàng #{orderToUpdate.OrderNumber}";
                        htmlMessage = EmailTemplateService.OrderCancelledEmail(
                            orderToUpdate,
                            orderToUpdate.User
                        );
                        break;
                }

                if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(htmlMessage))
                {
                    _emailQueue.QueueEmail(orderToUpdate.User.Email, subject, htmlMessage);
                }
            }

            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDto>(orderToUpdate);
        }

        public async Task HandlePayOSWebhook(WebhookData data)
        {
            // Chuyển đổi orderCode (số) từ PayOS thành chuỗi Hex để tìm trong DB
            string orderNumberToFind = data.orderCode.ToString("x").ToUpper();

            // Truy vấn trực tiếp đơn hàng bằng OrderNumber
            var foundOrder = await _unitOfWork.Order.GetAsync(
                o => o.OrderNumber.EndsWith(orderNumberToFind) && o.Status == OrderStatus.Pending,
                "Items.Product,User,Payment"
            );

            if (foundOrder != null && foundOrder.Payment?.Status != PaymentStatus.Accepted)
            {
                foundOrder.Status = OrderStatus.Confirmed;
                if (foundOrder.Payment != null)
                {
                    foundOrder.Payment.Status = PaymentStatus.Accepted;
                    foundOrder.Payment.PaidAt = DateTime.UtcNow;
                }

                if (foundOrder.User != null)
                {
                    var subject = $"[FlowerShop] Đã tiếp nhận đơn hàng #{foundOrder.OrderNumber}";
                    var subject_2 =
                        $"[FlowerShop] Thanh toán thành công cho đơn hàng #{foundOrder.OrderNumber}";
                    var htmlMessage = EmailTemplateService.OrderConfirmedEmail(
                        foundOrder,
                        foundOrder.User
                    );
                    var htmlMessage_2 = EmailTemplateService.PaymentSuccessEmail(
                        foundOrder,
                        foundOrder.User
                    );
                    _emailQueue.QueueEmail(foundOrder.User.Email, subject, htmlMessage);
                    _emailQueue.QueueEmail(foundOrder.User.Email, subject_2, htmlMessage_2);

                    var orderCreatedEvent = new OrderCreatedEvent
                    {
                        OrderNumber = foundOrder.OrderNumber,
                        TotalAmount = foundOrder.TotalAmount,
                        CustomerName = foundOrder.User?.FullName ?? "Khách hàng",
                    };
                    await _eventPublisher.PublishAsync(orderCreatedEvent);
                }

                await _unitOfWork.SaveAsync();
            }
        }

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
    }
}
