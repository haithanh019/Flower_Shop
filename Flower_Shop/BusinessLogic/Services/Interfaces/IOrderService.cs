using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Orders;
using Net.payOS.Types;

namespace BusinessLogic.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderFromCartAsync(Guid userId, OrderCreateRequest request);

        Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(Guid userId, QueryParameters queryParams);

        Task<OrderDto?> GetOrderDetailsAsync(Guid orderId);

        Task<OrderDto> UpdateOrderStatusAsync(OrderUpdateStatusRequest request);
        Task<PagedResultDto<OrderDto>> GetAllOrdersAsync(QueryParameters queryParams);
        Task HandlePayOSWebhook(WebhookData data);
    }
}
