using DataAccess.Entities;

namespace BusinessLogic.Services.Interfaces
{
    public interface IVietQRService
    {
        Task<string?> GenerateQRCode(Order order);
    }
}
