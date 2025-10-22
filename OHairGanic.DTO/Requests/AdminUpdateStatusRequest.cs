using System.ComponentModel.DataAnnotations;

namespace OHairGanic.DTO.Requests
{
    public class AdminUpdateStatusRequest
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; } // ví dụ: "CONFIRMED", "SHIPPED", "COMPLETED", "CANCELLED"

        [MaxLength(50)]
        public string? PaymentStatus { get; set; } // "PAID", "UNPAID", "REFUNDED" (optional)
    }
}
