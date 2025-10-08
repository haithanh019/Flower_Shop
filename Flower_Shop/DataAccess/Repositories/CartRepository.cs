using DataAccess.Data;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(FlowerShopDbContext db)
            : base(db) { }
    }
}
