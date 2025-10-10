using DataAccess.Data;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(FlowerShopDbContext db)
            : base(db) { }
    }
}
