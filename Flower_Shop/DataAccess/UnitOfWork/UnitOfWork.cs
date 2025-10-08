using DataAccess.Data;
using Microsoft.Extensions.Options;
using Ultitity.Options;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FlowerShopDbContext _db;

        public UnitOfWork(FlowerShopDbContext db, IOptions<CloudinaryOptions> cloudaryOptions)
        {
            _db = db;
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
