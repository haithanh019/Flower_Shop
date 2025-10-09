using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using Ultitity.Extensions;

namespace BusinessLogic.Services
{
    public class UtilityService : IUtilityService
    {
        public AllEnumsResponse GetAllEnums()
        {
            return new AllEnumsResponse
            {
                OrderStatus = EnumExtensions.GetEnumList<OrderStatus>(),
                PaymentMethod = EnumExtensions.GetEnumList<PaymentMethod>(),
                PaymentStatus = EnumExtensions.GetEnumList<PaymentStatus>(),
                UserRole = EnumExtensions.GetEnumList<UserRole>(),
            };
        }
    }
}
