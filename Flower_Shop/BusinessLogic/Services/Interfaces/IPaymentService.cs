using BusinessLogic.DTOs.Payments;

namespace BusinessLogic.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetPaymentByOrderIdAsync(Guid orderId);

        Task<PaymentDto> UpdatePaymentStatusAsync(PaymentUpdateStatusRequest request);
    }
}
