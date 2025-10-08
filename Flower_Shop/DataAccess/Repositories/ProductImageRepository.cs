using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataAccess.Data;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ultitity.Options;

namespace DataAccess.Repositories
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly FlowerShopDbContext _db;

        private readonly Cloudinary _cloudinary;

        public ProductImageRepository(FlowerShopDbContext db, IOptions<CloudinaryOptions> options)
            : base(db)
        {
            _db = db;

            var account = new Account(
                options.Value.CloudName,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task UploadImageAsync(IFormFile file, string folder, ProductImage image)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            image.PublicId = result.PublicId;
            image.Url = result.SecureUrl.ToString();
            _db.ProductImages.Add(image);
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return false;

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Error != null)
                return false;

            if (result.Result == "ok" || result.Result == "not found")
            {
                var image = await _db.ProductImages.FirstOrDefaultAsync(i =>
                    i.PublicId == publicId
                );
                if (image != null)
                {
                    _db.ProductImages.Remove(image);
                    await _db.SaveChangesAsync();
                }
                return true;
            }

            return false;
        }
    }
}
