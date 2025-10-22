using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
// PayOS SDK namespaces
using Net.payOS.Types;
using OHairGanic.BLL.Implementations;
using OHairGanic.DTO.Config;
using Net.payOS; // Các kiểu dữ liệu như PaymentData, ItemData [8, 9]

namespace OHairGanic.API.Controllers
{
    [ApiController]
    [Route("api/payment")] // Đường dẫn API
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly PayOSSettings _payosSettings;
        private readonly PayOS _payOS;

        public PaymentsController(IPaymentService paymentService, IOptions<PayOSSettings> payosOptions)
        {
            _paymentService = paymentService;
            // Lấy giá trị cấu hình PayOS
            _payosSettings = payosOptions.Value;

            // Khởi tạo đối tượng PayOS [6]
            _payOS = new PayOS(_payosSettings.ClientId, _payosSettings.ApiKey, _payosSettings.ChecksumKey);
        }

        // ... Các phương thức khác (GetById, Complete, v.v. như trong source [3, 4])

        /// <summary>
        /// Tạo link thanh toán PayOS
        /// </summary>
        [HttpPost("create-link")]
        public async Task<IActionResult> CreatePaymentLink(long orderCode, int amount, string description, string returnUrl, string cancelUrl)
        {
            try
            {
                // 1. Tạo ItemData (Thông tin sản phẩm) [8, 10]
                ItemData item = new ItemData("Đơn hàng thanh toán", 1, amount); // name, quantity, price [10]
                List<ItemData> items = new List<ItemData>();
                items.Add(item);

                // 2. Tạo PaymentData (Dữ liệu thanh toán) [8, 9]
                PaymentData paymentData = new PaymentData(
                    orderCode, // Mã đơn hàng [9]
                    amount, // Tổng số tiền [9]
                    description, // Mô tả giao dịch [9]
                    items, // Danh sách sản phẩm [9]
                    cancelUrl: cancelUrl, // URL khi hủy [9]
                    returnUrl: returnUrl // URL khi thành công [9]
                );

                // 3. Gọi phương thức tạo link thanh toán [8]
                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData); 

                // 4. Trả về Checkout URL cho người dùng [10]
                return Ok(new
                {
                    OrderCode = createPayment.orderCode,
                    CheckoutUrl = createPayment.checkoutUrl, // Đường dẫn đến trang thanh toán [10]
                    QrCodeData = createPayment.qrCode // Dữ liệu cho QR Code [10]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi tạo link thanh toán: " + ex.Message });
            }
        }

        /// <summary>
        /// Xử lý Webhook nhận thông tin thanh toán từ PayOS
        /// </summary>
        [AllowAnonymous] // Không yêu cầu xác thực [4]
        [HttpPost("webhook")] // Giả định đường dẫn Webhook [4]
        public async Task<IActionResult> PayOSWebhook([FromBody] JsonElement payload)
        {
            // Lấy body request [4]
            var body = payload.GetRawText();
            // Lấy chữ ký từ header [4]
            var signature = Request.Headers["x-signature"].ToString();

            // 1. Xác thực chữ ký (Signature Verification)
            // Sử dụng SecretKey (ChecksumKey) để tính toán chữ ký HMACSHA256 [4]
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_payosSettings.ChecksumKey)); // Sử dụng SecretKey [4]
            var computed = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)))
                .Replace("-", "").ToLowerInvariant();

            // So sánh chữ ký nhận được với chữ ký tính toán [5]
            if (signature != computed)
                return Unauthorized(new { message = "Invalid signature" });

            // 2. Phân tích cú pháp dữ liệu Webhook
            // Dữ liệu Webhook được chứa trong trường "data" [11, 12]
            if (!payload.TryGetProperty("data", out JsonElement dataElement))
            {
                return BadRequest(new { message = "Invalid webhook data structure." });
            }

            // Lấy Mã đơn hàng (orderCode) [13] và Mã trạng thái (code) [13]
            long orderCode = dataElement.GetProperty("orderCode").GetInt64();
            string statusCode = dataElement.GetProperty("code").GetString();

            // 3. Xử lý Logic Nghiệp vụ
            // Trạng thái thành công thường là "PAID" hoặc "SUCCESS" hoặc mã "00" [5, 12]
            if (statusCode == "PAID" || statusCode == "SUCCESS" || statusCode == "00")
            {
                // Thực hiện hoàn tất thanh toán (chuyển trạng thái đơn hàng) [5]
                await _paymentService.CompletePaymentAsync(orderCode);
            }

            return Ok(new { message = "Webhook verified and processed" });
        }
    }
}