using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Products;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            var product = await _unitOfWork.Product.GetAsync(
                filter: p => p.ProductId == productId,
                includeProperties: "Category,Images"
            );

            if (product == null)
            {
                return null;
            }

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateRequest createRequest)
        {
            // Kiểm tra nghiệp vụ: Giới hạn tối đa 5 ảnh
            if (createRequest.ImageFiles != null && createRequest.ImageFiles.Count > 5)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "ImageUrls", new[] { "A product can have a maximum of 5 images." } },
                    }
                );
            }

            var categoryExists = await _unitOfWork.Category.GetAsync(c =>
                c.CategoryId == createRequest.CategoryId
            );
            if (categoryExists == null)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "CategoryId", new[] { "The specified category does not exist." } },
                    }
                );
            }

            var productNameExists = await _unitOfWork.Product.GetAsync(p =>
                p.Name == createRequest.Name
            );
            if (productNameExists != null)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Name", new[] { "Product name already exists." } },
                    }
                );
            }

            var newProduct = _mapper.Map<Product>(createRequest);

            if (createRequest.ImageFiles != null && createRequest.ImageFiles.Count > 0)
            {
                foreach (var file in createRequest.ImageFiles)
                {
                    var image = new ProductImage();
                    // Giả sử bạn có một thư mục "flower_shop" trên Cloudinary
                    await _unitOfWork.ProductImage.UploadImageAsync(file, "flower_shop", image);
                    newProduct.Images.Add(image);
                }
            }

            await _unitOfWork.Product.AddAsync(newProduct);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ProductDto>(newProduct);
        }

        public async Task UpdateProductAsync(ProductUpdateRequest updateRequest)
        {
            // Kiểm tra nghiệp vụ: Giới hạn tối đa 5 ảnh khi cập nhật
            if (updateRequest.ImageFiles != null && updateRequest.ImageFiles.Count > 5)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "ImageUrls", new[] { "A product can have a maximum of 5 images." } },
                    }
                );
            }

            var productToUpdate = await _unitOfWork.Product.GetAsync(
                filter: p => p.ProductId == updateRequest.ProductId,
                includeProperties: "Images"
            );

            if (productToUpdate == null)
            {
                throw new KeyNotFoundException(
                    $"Product with ID {updateRequest.ProductId} not found."
                );
            }

            _mapper.Map(updateRequest, productToUpdate);

            // Xử lý ảnh: Xóa ảnh cũ và thêm ảnh mới nếu có
            if (updateRequest.ImageFiles != null && updateRequest.ImageFiles.Count > 0)
            {
                // Xóa tất cả ảnh cũ trên Cloudinary và DB
                foreach (var oldImage in productToUpdate.Images)
                {
                    if (!string.IsNullOrEmpty(oldImage.PublicId))
                    {
                        await _unitOfWork.ProductImage.DeleteImageAsync(oldImage.PublicId);
                    }
                }

                // Upload ảnh mới
                foreach (var file in updateRequest.ImageFiles)
                {
                    var newImage = new ProductImage();
                    await _unitOfWork.ProductImage.UploadImageAsync(file, "flower_shop", newImage);
                    productToUpdate.Images.Add(newImage);
                }
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            var productToDelete = await _unitOfWork.Product.GetAsync(p => p.ProductId == productId);
            if (productToDelete == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            _unitOfWork.Product.Remove(productToDelete);
            await _unitOfWork.SaveAsync();
        }

        public async Task<PagedResultDto<ProductDto>> GetProductsAsync(
            QueryParameters queryParameters
        )
        {
            var productsQuery = _unitOfWork.Product.GetQueryable(
                includeProperties: "Category,Images"
            );

            if (queryParameters.FilterCategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p =>
                    p.CategoryId == queryParameters.FilterCategoryId.Value
                );
            }

            if (!string.IsNullOrEmpty(queryParameters.Search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(queryParameters.Search));
            }

            // Logic sắp xếp được rút gọn
            productsQuery = queryParameters.SortBy?.ToLower() switch
            {
                "price" => queryParameters.SortDescending
                    ? productsQuery.OrderByDescending(p => p.Price)
                    : productsQuery.OrderBy(p => p.Price),
                _ => queryParameters.SortDescending // Mặc định sắp xếp theo Name
                    ? productsQuery.OrderByDescending(p => p.Name)
                    : productsQuery.OrderBy(p => p.Name),
            };

            var totalCount = await productsQuery.CountAsync();

            var pagedProducts = await productsQuery
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .ToListAsync();

            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDto>>(pagedProducts),
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
            };
        }
    }
}
