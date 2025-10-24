using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Responses
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public float TotalAmount { get; set; }
        public string? Provider { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderDetailResponse> Details { get; set; } = new();
    }

    public class OrderDetailResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }
    }

}
