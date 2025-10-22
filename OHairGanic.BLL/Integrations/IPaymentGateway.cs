namespace OHairGanic.BLL.Interfaces
{
    public interface IPaymentGateway
    {
        Task<(string qrUrl, string qrImage, DateTime expiresAt)> CreatePaymentAsync(int orderId, decimal amount);
    }
}
