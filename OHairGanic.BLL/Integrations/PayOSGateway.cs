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

        public async Task<(string qrUrl, string qrImage, DateTime expiresAt)> CreatePaymentAsync(int orderId, decimal amount)
        {
            var payload = new
            {
                amount = amount,
                description = $"Thanh toán đơn hàng #{orderId}",
                orderId = orderId.ToString(),
                currency = "VND",
                returnUrl = "https://ohairganic.vn/payment/success",
                cancelUrl = "https://ohairganic.vn/payment/cancel"
            };

            var json = JsonSerializer.Serialize(payload);
            // var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/v2/payment-link");
            // request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            // request.Headers.Add("x-client-id", _settings.ClientId);
            // request.Headers.Add("x-api-key", _settings.ApiKey);

            //  var response = await _httpClient.SendAsync(request);
            // var content = await response.Content.ReadAsStringAsync();

            //          if (!response.IsSuccessStatusCode)
            //     throw new Exception($"PayOS error: {content}");

            //  using var doc = JsonDocument.Parse(content);
            // var data = doc.RootElement.GetProperty("data");
            //var qrUrl = data.GetProperty("qrCodeUrl").GetString();
            //var qrImage = data.GetProperty("qrImage").GetString();
            //var expiresAt = DateTime.UtcNow.AddMinutes(10);

            //return (qrUrl!, qrImage!, expiresAt);
            return ("https://example.com/qr-url", "https://example.com/qr-image", DateTime.UtcNow.AddMinutes(10));
        }

    }
}
