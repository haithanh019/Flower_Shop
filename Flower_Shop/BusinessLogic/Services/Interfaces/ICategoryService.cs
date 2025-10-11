using BusinessLogic.DTOs.Categories;

namespace BusinessLogic.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

        Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId);

        Task<CategoryDto> CreateCategoryAsync(CategoryCreateRequest createRequest);

        Task<CategoryDto> UpdateCategoryAsync(CategoryUpdateRequest updateRequest);

        Task DeleteCategoryAsync(Guid categoryId);
    }
}
