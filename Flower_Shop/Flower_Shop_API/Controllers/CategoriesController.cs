using BusinessLogic.DTOs.Categories;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public CategoriesController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        [HttpGet]
        [AllowAnonymous] // Bất kỳ ai cũng có thể xem danh mục
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _facadeService.CategoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _facadeService.CategoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateRequest request)
        {
            var newCategory = await _facadeService.CategoryService.CreateCategoryAsync(request);
            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = newCategory.CategoryId },
                newCategory
            );
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateRequest request)
        {
            var updatedCategory = await _facadeService.CategoryService.UpdateCategoryAsync(request);
            return Ok(updatedCategory);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await _facadeService.CategoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
