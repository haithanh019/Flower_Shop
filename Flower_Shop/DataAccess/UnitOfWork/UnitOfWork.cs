using DataAccess.Data;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Ultitity.Options;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FlowerShopDbContext _db;

        public ICartRepository Cart { get; private set; }
        public ICartItemRepository CartItem { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderItemRepository OrderItem { get; private set; }
        public IPaymentRepository Payment { get; private set; }
        public IProductRepository Product { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }
        public IUserRepository User { get; private set; }

        public UnitOfWork(FlowerShopDbContext db, IOptions<CloudinaryOptions> cloudaryOptions)
        {
            _db = db;
            Cart = new CartRepository(_db);
            CartItem = new CartItemRepository(_db);
            Category = new CategoryRepository(_db);
            Order = new OrderRepository(_db);
            OrderItem = new OrderItemRepository(_db);
            Payment = new PaymentRepository(_db);
            Product = new ProductRepository(_db);
            ProductImage = new ProductImageRepository(_db, cloudaryOptions);
            User = new UserRepository(_db);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
