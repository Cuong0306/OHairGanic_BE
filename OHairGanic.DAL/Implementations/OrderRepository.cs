using Microsoft.EntityFrameworkCore;
using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OHairGanicDBContext _context;
        public OrderRepository(OHairGanicDBContext context)
        {
            _context = context;
        }

        public async Task AddOrderAsync(Order order)
        {
            // EF sẽ tự add cả OrderDetails qua navigation
            await _context.Orders.AddAsync(order);
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)      // để có ProductName khi map response
                .Include(o => o.Payments)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)// để tính TotalAmount .Sum(d => d.Price * d.Quantity)
                .Include(o => o.Payments)
                .Include(o => o.User)
                .ToListAsync();
        }
        public async Task<List<Order>> GetPaidOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payments)
                .Include(o => o.User)
                .Where(o => o.Payments.Any(p => p.Status == "PAID"))
                .ToListAsync();
        }

    }
}
