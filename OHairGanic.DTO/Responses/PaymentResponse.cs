namespace OHairGanic.DTO.Responses
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Provider { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        // ✅ thêm 2 field này cho PayOS
        public string QrPayload { get; set; }
        public string QrImagePath { get; set; }
    }
}
