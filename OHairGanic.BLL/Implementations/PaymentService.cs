using OHairGanic.BLL.Integrations;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Responses;

namespace OHairGanic.BLL.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateway _gateway;

        public PaymentService(IUnitOfWork unitOfWork, IPaymentGateway gateway)
        {
            _unitOfWork = unitOfWork;
            _gateway = gateway;
        }

        // ============ 1. Tạo giao dịch thanh toán ============
        public async Task<PaymentResponse> CreatePaymentAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId)
                ?? throw new Exception("Order not found");

            // Gọi PayOS API
            var (qrUrl, qrImage, expiresAt) = await _gateway.CreatePaymentAsync(orderId, (decimal)order.TotalAmount);

            var payment = new DAL.Models.Payment
            {
                OrderId = orderId,
                Amount = order.TotalAmount,
                Currency = "VND",
                Provider = "PayOS",
                QrPayload = qrUrl,
                QrImagePath = qrImage,
                ExpiresAt = expiresAt,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payments.AddPaymentAsync(payment);
            await _unitOfWork.SaveAsync();

            return new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = (decimal)payment.Amount,
                Currency = payment.Currency,
                Provider = payment.Provider,
                QrPayload = payment.QrPayload,
                QrImagePath = payment.QrImagePath,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt
            };
        }

        // ============ 2. Hoàn tất thanh toán ============
        public async Task<object> CompletePaymentAsync(long id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentByIdAsync(id)
                ?? throw new Exception("Payment not found");

            payment.Status = "SUCCESS";
            payment.PaidAt = DateTime.UtcNow;

            var order = await _unitOfWork.Orders.GetOrderByIdAsync(payment.OrderId);
            if (order != null)
                order.Status = "PAID";

            await _unitOfWork.SaveAsync();

            return new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = (decimal)payment.Amount,
                Currency = payment.Currency,
                Provider = payment.Provider,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt
            };
        }

        // ============ 3. Lấy chi tiết thanh toán ============
        public async Task<PaymentResponse> GetPaymentByIdAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentByIdAsync(id)
                ?? throw new Exception("Payment not found");

            return new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = (decimal)payment.Amount,
                Currency = payment.Currency,
                Provider = payment.Provider,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt
            };
        }
    }
}
