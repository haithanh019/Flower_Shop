using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Products;

namespace BusinessLogic.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResultDto<ProductDto>> GetProductsAsync(QueryParameters queryParameters);
        Task<ProductDto?> GetProductByIdForAdminAsync(Guid productId);

        Task<ProductDto?> GetProductByIdAsync(Guid productId);

        Task<ProductDto> CreateProductAsync(ProductCreateRequest createRequest);

        Task<ProductDto> UpdateProductAsync(ProductUpdateRequest updateRequest);
        Task<bool> DeleteProductImageAsync(ProductImageDeleteRequest request);
        Task DeleteProductAsync(Guid productId);
    }
}
