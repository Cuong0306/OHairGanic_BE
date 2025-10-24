using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;

namespace OHairGanic.API.Controllers
{
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ==================== LẤY TẤT CẢ ====================
        [Authorize]
        [HttpGet(ApiRoutes.Product.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                if (products == null || !products.Any())
                    return NotFound(new { message = "No available products." });

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== LẤY THEO ID ====================
        [Authorize]
        [HttpGet(ApiRoutes.Product.GetById)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = "Product not found." });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== TẠO MỚI ====================
        [Authorize]
        [HttpPost(ApiRoutes.Product.Create)]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest dto)
        {
            try
            {
                var result = await _productService.AddProductAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== CẬP NHẬT ====================
        [Authorize]
        [HttpPut(ApiRoutes.Product.Update)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest dto)
        {
            try
            {
                var updated = await _productService.UpdateProductAsync(id, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== XÓA ====================
        [Authorize]
        [HttpDelete(ApiRoutes.Product.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                    return BadRequest("Failed to delete product.");

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== LỌC THEO KÝ TỰ ĐẦU ====================
        [Authorize]
        [HttpGet(ApiRoutes.Product.GetByInitial)]
        public async Task<IActionResult> GetByInitial([FromRoute] string? initial)
        {
            try
            {
                // Không truyền -> trả all
                if (string.IsNullOrWhiteSpace(initial))
                {
                    var all = await _productService.GetAllProductsAsync();
                    if (all == null || !all.Any())
                        return NotFound(new { message = "No available products." });
                    return Ok(all);
                }

                // Lấy đúng 1 ký tự đầu để lọc
                char first = initial.Trim()[0];

                var list = await _productService.GetProductsByInitialAsync(first);

                // Fallback: không có kết quả -> trả all
                if (list == null || list.Count == 0)
                {
                    var all = await _productService.GetAllProductsAsync();
                    if (all == null || !all.Any())
                        return NotFound(new { message = "No available products." });
                    return Ok(all);
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
