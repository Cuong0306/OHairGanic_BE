using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public interface IPaymentService
    {
        Task<object> CompletePaymentAsync(long orderId);
        Task<PaymentResponse> GetPaymentByIdAsync(int id);
        Task<PaymentResponse> CreatePaymentAsync(int orderId);

        Task<List<PaymentResponse>> GetAllPaymentsAsync();
        Task<List<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId);
    }
}
