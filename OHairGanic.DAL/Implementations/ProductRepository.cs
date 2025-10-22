using Microsoft.EntityFrameworkCore;
using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly OHairGanicDBContext _context;
        public ProductRepository(OHairGanicDBContext context)
        {
            _context = context;
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
            // Hoặc chỉ hàng đang bán: return await _context.Products.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.Distinct().ToList();
            return await _context.Products
                .Where(p => idList.Contains(p.Id))
                .ToListAsync();
        }

        // ✅ Cập nhật đầy đủ, có validate
        public async Task<bool> UpdateProductAsync(Product product)
        {
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existing == null) throw new System.Exception("Product not found");

            // Patch các string nếu có giá trị
            if (!string.IsNullOrWhiteSpace(product.Name))
                existing.Name = product.Name;
            if (!string.IsNullOrWhiteSpace(product.Tags))
                existing.Tags = product.Tags;
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                existing.ImageUrl = product.ImageUrl;

            // Giá: cho phép 0, chặn âm
            if (product.Price < 0) throw new System.Exception("Price cannot be negative.");
            existing.Price = product.Price;

            // ✅ Stock: CHẮC CHẮN cập nhật & chặn âm
            if (product.Stock < 0) throw new System.Exception("Stock cannot be negative.");
            existing.Stock = product.Stock;

            // ✅ Tự động bật/tắt theo tồn kho (nếu muốn điều khiển thủ công, dùng dòng dưới thay thế)
            existing.IsActive = existing.Stock > 0;
            // existing.IsActive = product.IsActive;

            _context.Products.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }

        // ✅ (Khuyên dùng) Giảm kho an toàn: chỉ trừ khi đủ hàng; trả về true/false
        public async Task<bool> TryDecreaseStockAsync(int productId, int quantity)
        {
            if (quantity <= 0) throw new System.Exception("Quantity must be > 0.");

            // Tải entity, kiểm đủ hàng, trừ kho, auto toggle IsActive
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (existing == null) return false;
            if (existing.Stock < quantity) return false;

            existing.Stock -= quantity;
            existing.IsActive = existing.Stock > 0;

            _context.Products.Update(existing);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
