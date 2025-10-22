using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest dto, int userId);
        Task<OrderResponse> GetOrderByIdAsync(int id);
        Task<List<OrderResponse>> GetAllOrdersAsync();
        Task<AdminUpdateStatusResponse> AdminUpdateStatusAsync(AdminUpdateStatusRequest request);

    }
}
