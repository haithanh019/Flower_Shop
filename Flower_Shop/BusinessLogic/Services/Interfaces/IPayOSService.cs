using DataAccess.Entities;
using Net.payOS.Types;

namespace BusinessLogic.Services.Interfaces
{
    public interface IPayOSService
    {
        Task<CreatePaymentResult?> CreatePaymentLink(Order order);
        WebhookData VerifyPaymentWebhook(WebhookType webhook);
    }
}
