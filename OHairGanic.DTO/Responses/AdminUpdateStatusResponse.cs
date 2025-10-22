using System;

namespace OHairGanic.DTO.Responses
{
    public class AdminUpdateStatusResponse
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
