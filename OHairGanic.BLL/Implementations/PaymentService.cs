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

            // 🔹 1. Nếu order đã thanh toán, chặn lại
            if (order.Status == "PAID")
                throw new InvalidOperationException("Order already paid, cannot create payment link.");

            // 🔹 2. Lấy giao dịch gần nhất của order
            var payments = await _unitOfWork.Payments.GetPaymentsByOrderIdAsync(orderId);
            var latest = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            // 🔹 3. Nếu có payment đang chờ (PENDING) và chưa hết hạn → trả lại link cũ
            if (latest != null && latest.Status == "PENDING" && latest.ExpiresAt > DateTime.UtcNow)
            {
                return new PaymentResponse
                {
                    Id = latest.Id,
                    OrderId = latest.OrderId,
                    Amount = (decimal)latest.Amount,
                    Currency = latest.Currency,
                    Provider = latest.Provider,
                    Status = latest.Status,
                    CreatedAt = latest.CreatedAt,
                    PaidAt = latest.PaidAt
                };
            }

            // 🔹 4. Nếu payment cũ đã hết hạn hoặc cancelled → tạo mới
            int orderCode = order.Id;
            try
            {
                var (qrUrl, qrImage, expiresAt, payosStatus) = await _gateway.CreatePaymentAsync(orderCode, (decimal)order.TotalAmount);

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
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                // 🔹 5. Nếu PayOS báo trùng orderCode → sinh mã mới (an toàn)
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    orderCode = int.Parse($"{order.Id}{DateTime.UtcNow:HHmmss}");
                    var (qrUrl, qrImage, expiresAt, payosStatus) = await _gateway.CreatePaymentAsync(orderCode, (decimal)order.TotalAmount);

                    var safeExpiry = expiresAt > DateTime.UtcNow
                        ? expiresAt
                        : DateTime.UtcNow.AddMinutes(30);

                    var payment = new DAL.Models.Payment
                    {
                        OrderId = orderId,
                        Amount = order.TotalAmount,
                        Currency = "VND",
                        Provider = "PayOS",
                        QrPayload = qrUrl,
                        QrImagePath = qrImage,
                        ExpiresAt = safeExpiry,
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
                        Status = payment.Status,
                        CreatedAt = payment.CreatedAt
                    };
                }

                throw;
            }
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

        // ============ 4. Lấy tất cả thanh toán ============
        public async Task<List<PaymentResponse>> GetAllPaymentsAsync()
        {
            var payments = await _unitOfWork.Payments.GetAllPaymentsAsync();
            return payments.Select(p => new PaymentResponse
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = (decimal)p.Amount,
                Currency = p.Currency,
                Provider = p.Provider,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt
            }).ToList();
        }

        // ============ 5. Lấy theo OrderId ============
        public async Task<List<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByOrderIdAsync(orderId);
            return payments.Select(p => new PaymentResponse
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = (decimal)p.Amount,
                Currency = p.Currency,
                Provider = p.Provider,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt
            }).ToList();
        }
    }
}

