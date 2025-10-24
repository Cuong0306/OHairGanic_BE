using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DTO.Requests
{
    public class CreateOrderRequest
    {
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public string? Provider { get; set; } 
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
