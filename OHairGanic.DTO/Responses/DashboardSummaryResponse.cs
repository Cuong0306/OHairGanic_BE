using System;
using System.Collections.Generic;

namespace OHairGanic.DTO.Responses
{
    public class DashboardSummaryResponse
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public float TotalRevenue { get; set; }

        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<MonthlyOrderDto> MonthlyOrders { get; set; } = new();

        // ✅ thêm danh sách đơn hàng đã thanh toán
        public List<OrderSummaryDto> Orders { get; set; } = new();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public float Revenue { get; set; }
    }

    public class MonthlyOrderDto
    {
        public string Month { get; set; } = string.Empty;
        public int Orders { get; set; }
    }

    // ✅ DTO tóm tắt đơn hàng cho FE
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public float TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
