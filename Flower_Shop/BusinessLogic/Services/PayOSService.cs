using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;

namespace BusinessLogic.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly ILogger<PayOSService> _logger;
        private readonly IConfiguration _configuration;

        public PayOSService(IConfiguration configuration, ILogger<PayOSService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            var clientId =
                configuration["PayOS:ClientId"]
                ?? throw new ArgumentNullException("PayOS:ClientId");
            var apiKey =
                configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("PayOS:ApiKey");
            var checksumKey =
                configuration["PayOS:ChecksumKey"]
                ?? throw new ArgumentNullException("PayOS:ChecksumKey");

            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        public async Task<CreatePaymentResult?> CreatePaymentLink(Order order)
        {
            try
            {
                // Order code phải là số nguyên, ta có thể dùng một phần timestamp hoặc hashcode
                long orderCode = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var items = new List<ItemData>();
                foreach (var item in order.Items)
                {
                    items.Add(
                        new ItemData(
                            item.Product?.Name ?? "Sản phẩm",
                            item.Quantity,
                            (int)item.UnitPrice
                        )
                    );
                }

                PaymentData paymentData = new PaymentData(
                    orderCode,
                    (int)order.TotalAmount,
                    $"Thanh toán đơn hàng #{order.OrderNumber}",
                    items,
                    _configuration["WebApp:BaseUrl"] + "/Orders/PaymentCancelled",
                    _configuration["WebApp:BaseUrl"] + "/Orders/PaymentSuccess"
                );

                CreatePaymentResult createPaymentResult = await _payOS.createPaymentLink(
                    paymentData
                );
                return createPaymentResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi khi tạo link thanh toán PayOS cho đơn hàng #{OrderNumber}",
                    order.OrderNumber
                );
                return null;
            }
        }

        public WebhookData VerifyPaymentWebhook(WebhookType webhook)
        {
            return _payOS.verifyPaymentWebhookData(webhook);
        }
    }
}
