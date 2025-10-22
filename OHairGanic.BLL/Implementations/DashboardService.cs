using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Responses;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryResponse> GetSummaryAsync()
        {
            var users = await _unitOfWork.Users.GetAllActiveUsersAsync();
            var products = await _unitOfWork.Products.GetAllProductsAsync();
            var orders = await _unitOfWork.Orders.GetAllOrdersAsync();

            // ✅ Lọc các đơn đã thanh toán (dựa vào Payment.Status == "PAID")
            var paidOrders = orders
                .Where(o => o.Payments != null && o.Payments.Any(p => p.Status == "PAID"))
                .ToList();

            // ✅ Tính tổng doanh thu chỉ từ đơn đã thanh toán
            var totalRevenue = paidOrders.Sum(o => o.TotalAmount);

            // ✅ Gom nhóm doanh thu theo tháng (chỉ tính PAID)
            var monthlyRevenue = paidOrders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"T{g.Key.Month}",
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .TakeLast(6)
                .ToList();

            // ✅ Gom nhóm số đơn hàng theo tháng (tính tất cả đơn)
            var monthlyOrders = orders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyOrderDto
                {
                    Month = $"T{g.Key.Month}",
                    Orders = g.Count()
                })
                .TakeLast(6)
                .ToList();

            // ✅ Trả thêm danh sách đơn hàng (để FE dễ render)
            var orderDtos = paidOrders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                PaymentStatus = o.Payments.FirstOrDefault()?.Status ?? "UNPAID",
                CreatedAt = o.CreatedAt
            }).ToList();

            return new DashboardSummaryResponse
            {
                TotalUsers = users.Count,
                TotalProducts = products.Count,
                TotalOrders = orders.Count,
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                MonthlyOrders = monthlyOrders,
                Orders = orderDtos
            };
        }
    }
}
