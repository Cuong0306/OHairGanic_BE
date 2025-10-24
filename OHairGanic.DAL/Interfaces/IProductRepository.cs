using OHairGanic.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProductsAsync();                 // list: không nullable
        Task<Product?> GetProductByIdAsync(int productId);         // single: có thể null
        Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids);   // ✅ thêm cho validate theo lô

        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int productId);

        Task<List<Product>> GetProductsByInitialAsync(char initial);
    }
}
