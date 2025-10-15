using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Events;
using BusinessLogic.Services;
using BusinessLogic.Services.Interfaces;

namespace Flower_Shop_API.Services
{
    public class NotificationHandler : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationHandler> _logger;

        public NotificationHandler(
            IServiceProvider serviceProvider,
            ILogger<NotificationHandler> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InMemoryEventBus.OnPublish += HandleEvent;
            return Task.CompletedTask;
        }

        private async Task HandleEvent(object? eventData)
        {
            if (eventData is OrderCreatedEvent orderEvent)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService =
                        scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var message =
                        $"🔔 *Đơn hàng mới!*\n\n"
                        + $"Mã ĐH: *#{orderEvent.OrderNumber}*\n"
                        + $"Khách hàng: `{orderEvent.CustomerName}`\n"
                        + $"Tổng tiền: *{orderEvent.TotalAmount:N0} VNĐ*";

                    try
                    {
                        await notificationService.SendMessageAsync(message);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error sending notification for order {OrderNumber}",
                            orderEvent.OrderNumber
                        );
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            InMemoryEventBus.OnPublish -= HandleEvent;
            return Task.CompletedTask;
        }
    }
}
