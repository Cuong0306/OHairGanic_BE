using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Config;

namespace OHairGanic.BLL.Integrations
{
    public class PayOSGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly PayOSSettings _settings;

        public PayOSGateway(HttpClient httpClient, IOptions<PayOSSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<(string qrUrl, string qrImage, DateTime expiresAt, string status)>
    CreatePaymentAsync(int orderId, decimal amount)
        {
            long orderCode = long.Parse($"{orderId}{DateTime.UtcNow:HHmmss}");
            var payload = new
            {
                orderCode,                 // long, duy nhất
                amount = (int)amount,      // PayOS nhận int
                description = $"Thanh toán đơn hàng #{orderId}",
                currency = "VND",
                returnUrl = "https://ohairganic.vn/payment/success",
                cancelUrl = "https://ohairganic.vn/payment/cancel"
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/payment-requests")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            req.Headers.Add("x-client-id", _settings.ClientId);
            req.Headers.Add("x-api-key", _settings.ApiKey);

            var resp = await _httpClient.SendAsync(req);
            var content = await resp.Content.ReadAsStringAsync();

            // Nếu HTTP không thành công -> ném lỗi luôn, kèm body PayOS
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"PayOS error: HTTP {(int)resp.StatusCode} - {content}");

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // Một số response có dạng { "code": "00", "desc": "...", "data": {...} }
            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            {
                // data null/không phải object -> ném lỗi mô tả rõ
                throw new Exception($"PayOS response invalid (no data object). Body={content}");
            }

            string? checkoutUrl = null;
            string? qrImage = null;
            string status = "PENDING";

            if (data.TryGetProperty("checkoutUrl", out var coutEl) && coutEl.ValueKind == JsonValueKind.String)
                checkoutUrl = coutEl.GetString();

            if (data.TryGetProperty("qrCode", out var qrEl) && qrEl.ValueKind == JsonValueKind.String)
                qrImage = qrEl.GetString();

            if (data.TryGetProperty("status", out var stEl) && stEl.ValueKind == JsonValueKind.String)
                status = stEl.GetString() ?? "PENDING";

            if (string.IsNullOrEmpty(checkoutUrl))
                throw new Exception($"PayOS response missing checkoutUrl. Body={content}");

            var expiresAt = DateTime.UtcNow.AddMinutes(30);
            return (checkoutUrl!, qrImage ?? string.Empty, expiresAt, status);
        }

    }
}