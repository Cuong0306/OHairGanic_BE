using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest dto, int userId)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("Order must contain at least one product.");

            var merged = dto.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            var ids = merged.Select(x => x.ProductId).ToList();
            var products = await _unitOfWork.Products.GetByIdsAsync(ids);
            var dict = products.ToDictionary(p => p.Id);

            float totalAmount = 0;
            var order = new Order
            {
                UserId = userId,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };

            foreach (var m in merged)
            {
                var prod = dict[m.ProductId];
                if (!prod.IsActive || prod.Stock < m.Quantity)
                    throw new InvalidOperationException($"Product {prod.Name} is not available or out of stock.");

                prod.Stock -= m.Quantity;
                if (prod.Stock == 0) prod.IsActive = false;
                await _unitOfWork.Products.UpdateProductAsync(prod);

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = prod.Id,
                    Quantity = m.Quantity,
                    Price = prod.Price
                });

                totalAmount += prod.Price * m.Quantity;
            }

            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.AddOrderAsync(order);
            await _unitOfWork.SaveAsync();

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = totalAmount,
                Currency = "VND",
                Provider = dto.Provider ?? "CASH",
                Status = "UNPAID",
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Payments.AddPaymentAsync(payment);
            await _unitOfWork.SaveAsync();

            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User?.FullName ?? "Ẩn danh",
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                TotalAmount = totalAmount,
                PaymentStatus = payment.Status,
                Details = order.OrderDetails.Select(d => new OrderDetailResponse
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Price = d.Price,
                    Quantity = d.Quantity
                }).ToList()
            };
        }

        public async Task<OrderResponse> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(id);
            if (order == null)
                throw new Exception("Order not found.");

            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User?.FullName ?? "Ẩn danh",
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.OrderDetails.Sum(d => d.Price * d.Quantity),
                PaymentStatus = order.Payments.FirstOrDefault()?.Status ?? "UNKNOWN",
                Details = order.OrderDetails.Select(d => new OrderDetailResponse
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Price = d.Price,
                    Quantity = d.Quantity
                }).ToList()
            };
        }

        public async Task<List<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllOrdersAsync();
            return orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                Status = o.Status,
                CustomerName = o.User?.FullName ?? "Ẩn danh",
                CreatedAt = o.CreatedAt,
                TotalAmount = o.OrderDetails.Sum(d => d.Price * d.Quantity),
                PaymentStatus = o.Payments.FirstOrDefault()?.Status ?? "UNKNOWN",
                Details = o.OrderDetails.Select(d => new OrderDetailResponse
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Price = d.Price,
                    Quantity = d.Quantity
                }).ToList()
            }).ToList();
        }
        public async Task<AdminUpdateStatusResponse> AdminUpdateStatusAsync(AdminUpdateStatusRequest request)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception("Order not found.");

            // Cập nhật trạng thái đơn hàng
            order.Status = request.OrderStatus;

            // Cập nhật trạng thái thanh toán nếu có
            if (!string.IsNullOrEmpty(request.PaymentStatus))
            {
                var payment = order.Payments.FirstOrDefault();
                if (payment != null)
                {
                    payment.Status = request.PaymentStatus;
                    if (request.PaymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        payment.PaidAt = DateTime.UtcNow;
                    }
                }
            }

            await _unitOfWork.SaveAsync();

            return new AdminUpdateStatusResponse
            {
                OrderId = order.Id,
                OrderStatus = order.Status,
                PaymentStatus = order.Payments.FirstOrDefault()?.Status ?? "UNKNOWN",
                UpdatedAt = DateTime.UtcNow
            };
        }

    }
}
