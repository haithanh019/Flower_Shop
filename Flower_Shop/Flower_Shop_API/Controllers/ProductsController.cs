using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Products;
using BusinessLogic.Services.FacadeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flower_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IFacadeService _facadeService;

        public ProductsController(IFacadeService facadeService)
        {
            _facadeService = facadeService;
        }

        // Endpoint cho tất cả mọi người có thể xem sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] QueryParameters queryParams)
        {
            var products = await _facadeService.ProductService.GetProductsAsync(queryParams);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _facadeService.ProductService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // Các endpoint dưới đây yêu cầu quyền Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateRequest request)
        {
            var newProduct = await _facadeService.ProductService.CreateProductAsync(request);
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = newProduct.ProductId },
                newProduct
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(
            Guid id,
            [FromForm] ProductUpdateRequest request
        )
        {
            if (id != request.ProductId)
            {
                return BadRequest("Product ID mismatch.");
            }
            await _facadeService.ProductService.UpdateProductAsync(request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _facadeService.ProductService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
