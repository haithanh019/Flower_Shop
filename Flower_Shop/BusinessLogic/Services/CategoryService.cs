using AutoMapper;
using BusinessLogic.DTOs.Categories;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Ultitity.Exceptions;

namespace BusinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var allCategories = await _unitOfWork.Category.GetAllAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(allCategories);
            return categoryDtos;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId)
        {
            var category = await _unitOfWork.Category.GetAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Category", new[] { "Category not found." } },
                    }
                );
            }

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateRequest createRequest)
        {
            var existingCategory = await _unitOfWork.Category.GetAsync(c =>
                c.Name == createRequest.Name
            );
            if (existingCategory != null)
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        { "Category", new[] { "Category name already exists." } },
                    }
                );
            }

            var newCategory = _mapper.Map<Category>(createRequest);

            await _unitOfWork.Category.AddAsync(newCategory);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<CategoryDto>(newCategory);
        }

        public async Task UpdateCategoryAsync(CategoryUpdateRequest updateRequest)
        {
            var categoryToUpdate = await _unitOfWork.Category.GetAsync(c =>
                c.CategoryId == updateRequest.CategoryId
            );
            if (categoryToUpdate == null)
            {
                // Ném một exception chung khi không tìm thấy đối tượng để CẬP NHẬT/XÓA.
                // Middleware sẽ bắt lỗi này và trả về 500 Internal Server Error.
                throw new KeyNotFoundException(
                    $"Category with ID {updateRequest.CategoryId} not found."
                );
            }

            _mapper.Map(updateRequest, categoryToUpdate);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var categoryToDelete = await _unitOfWork.Category.GetAsync(c =>
                c.CategoryId == categoryId
            );
            if (categoryToDelete == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            }

            var productsInCategory = await _unitOfWork.Product.GetRangeAsync(p =>
                p.CategoryId == categoryId
            );
            if (productsInCategory.Any())
            {
                throw new CustomValidationException(
                    new Dictionary<string, string[]>
                    {
                        {
                            "Integrity",
                            new[] { "Cannot delete category because it contains products." }
                        },
                    }
                );
            }

            _unitOfWork.Category.Remove(categoryToDelete);
            await _unitOfWork.SaveAsync();
        }
    }
}
