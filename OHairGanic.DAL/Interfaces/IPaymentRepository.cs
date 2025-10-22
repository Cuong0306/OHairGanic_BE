using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddPaymentAsync(Payment payment);
        Task<Payment> GetPaymentByIdAsync(long id);
        Task<List<Payment>> GetPaymentsByOrderIdAsync(int orderId);
    }
}
