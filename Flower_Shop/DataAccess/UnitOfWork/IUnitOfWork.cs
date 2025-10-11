using DataAccess.Repositories.Interfaces;

namespace DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        ICartRepository Cart { get; }
        ICartItemRepository CartItem { get; }
        ICategoryRepository Category { get; }
        IOrderRepository Order { get; }
        IOrderItemRepository OrderItem { get; }
        IPaymentRepository Payment { get; }
        IProductRepository Product { get; }
        IProductImageRepository ProductImage { get; }
        IUserRepository User { get; }
        IAddressRepository Address { get; }
        Task SaveAsync();
    }
}
