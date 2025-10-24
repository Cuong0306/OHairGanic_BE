using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public class ProductService : IProductService
    {
        private const int STOCK_MAX = 1_000_000;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static int NormalizeAndValidateStock(int? stock)
        {
            // Null => 0
            var value = stock ?? 0;

            // Chặn âm, chặn vượt ngưỡng phi lý
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(stock), "Stock must be >= 0.");

            if (value > STOCK_MAX)
                throw new ArgumentOutOfRangeException(nameof(stock), $"Stock must be <= {STOCK_MAX}.");

            return value;
        }

        public async Task<ProductResponse> AddProductAsync(CreateProductRequest dto)
        {
            var safeStock = NormalizeAndValidateStock(dto.Stock);

            var newProduct = new Product
            {
                Name = dto.ProductName,
                Tags = dto.Tags,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                Stock = safeStock,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Products.AddProductAsync(newProduct);

            return new ProductResponse
            {
                ProductId = newProduct.Id,
                ProductName = newProduct.Name,
                Tags = newProduct.Tags,
                Price = newProduct.Price,
                ImageUrl = newProduct.ImageUrl,
                Stock = newProduct.Stock,
                CreatedAt = newProduct.CreatedAt,
                IsActive = newProduct.IsActive
            };
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var result = await _unitOfWork.Products.DeleteProductAsync(id);
            if (!result)
                throw new Exception("Failed to delete Product");
            return true;
        }

        public async Task<List<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllProductsAsync();
            return products.Select(p => new ProductResponse
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Tags = p.Tags,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock,
                CreatedAt = DateTime.UtcNow,
                IsActive = p.IsActive
            }).ToList();
        }

        public Task<ProductResponse> GetProductByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductResponse> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetProductByIdAsync(id);
            if (product == null)
                throw new Exception("Product not found");

            return new ProductResponse
            {
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                Price = product.Price,
                ProductId = product.Id,
                ProductName = product.Name,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt,
                Tags = product.Tags
            };
        }

        public async Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest dto)
        {
            var product = await _unitOfWork.Products.GetProductByIdAsync(id);
            if (product == null)
                throw new Exception($"Product with ID {id} not found.");

            if (!string.IsNullOrEmpty(dto.Name))
                product.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Tags))
                product.Tags = dto.Tags;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                product.ImageUrl = dto.ImageUrl;

            // ✅ Validate stock trước khi gán
            if (dto.Stock.HasValue)
                product.Stock = NormalizeAndValidateStock(dto.Stock);

            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            var success = await _unitOfWork.Products.UpdateProductAsync(product);
            if (!success)
                throw new Exception("Failed to update product.");

            return new ProductResponse
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Tags = product.Tags,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt,
                IsActive = product.IsActive
            };
        }
        public async Task<List<ProductResponse>> GetProductsByInitialAsync(char initial)
        {
            var products = await _unitOfWork.Products.GetProductsByInitialAsync(initial);

            return products.Select(p => new ProductResponse
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Tags = p.Tags,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock,
                CreatedAt = p.CreatedAt, // (nhỏ) bạn nên map từ DB, đừng dùng UtcNow ở GetAll
                IsActive = p.IsActive
            }).ToList();
        }

    }
}
