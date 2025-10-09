using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Products;

namespace BusinessLogic.Services.Interfaces
{
    public interface IProductService
    {
        Task<PagedResultDto<ProductDto>> GetProductsAsync(QueryParameters queryParameters);

        Task<ProductDto?> GetProductByIdAsync(Guid productId);

        Task<ProductDto> CreateProductAsync(ProductCreateRequest createRequest);

        Task UpdateProductAsync(ProductUpdateRequest updateRequest);

        Task DeleteProductAsync(Guid productId);
    }
}
