using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetAllProductsAsync();
        Task<ProductResponse> GetProductByIdAsync(int id);
        Task<ProductResponse> GetProductByNameAsync(string name);
        Task<ProductResponse> AddProductAsync(CreateProductRequest dto);
        Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest dto);
        Task<bool> DeleteProductAsync(int id);
    }
}
