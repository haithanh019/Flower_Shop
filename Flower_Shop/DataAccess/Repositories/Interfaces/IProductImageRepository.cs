using DataAccess.Entities;
using Microsoft.AspNetCore.Http;

namespace DataAccess.Repositories.Interfaces
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        Task UploadImageAsync(IFormFile file, string folder, ProductImage image);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
