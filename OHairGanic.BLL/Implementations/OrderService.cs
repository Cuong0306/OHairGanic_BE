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

        // Helper: chọn payment “chính” (ưu tiên đã thanh toán, nếu không có thì mới tạo nhất)
        private static Payment? SelectPrimaryPayment(IEnumerable<Payment> payments)
        {
            return payments?
                .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)
                .ThenByDescending(p => p.CreatedAt)
                .FirstOrDefault();
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
                Provider = payment.Provider, // 🆕 trả thêm provider ngay khi tạo
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
                throw new KeyNotFoundException("Order not found."); // 🆕 để Controller trả 404

            var payment = SelectPrimaryPayment(order.Payments);

            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User?.FullName ?? "Ẩn danh",
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.OrderDetails.Sum(d => d.Price * d.Quantity),
                PaymentStatus = payment?.Status ?? "UNKNOWN",
                Provider = payment?.Provider, // 🆕 thêm provider
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
            return orders.Select(o =>
            {
                var payment = SelectPrimaryPayment(o.Payments);
                return new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Status = o.Status,
                    CustomerName = o.User?.FullName ?? "Ẩn danh",
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.OrderDetails.Sum(d => d.Price * d.Quantity),
                    PaymentStatus = payment?.Status ?? "UNKNOWN",
                    Provider = payment?.Provider, // 🆕 đồng bộ luôn ở list
                    Details = o.OrderDetails.Select(d => new OrderDetailResponse
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.Name ?? "",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                };
            }).ToList();
        }

        public async Task<AdminUpdateStatusResponse> AdminUpdateStatusAsync(AdminUpdateStatusRequest request)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(request.OrderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found."); // 🆕 thống nhất

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

        public async Task<List<OrderResponse>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            return orders.Select(o =>
            {
                var payment = SelectPrimaryPayment(o.Payments);
                return new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CustomerName = o.User?.FullName ?? "Ẩn danh",
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.OrderDetails.Sum(d => d.Price * d.Quantity),
                    PaymentStatus = payment?.Status ?? "UNKNOWN",
                    Provider = payment?.Provider, // 🆕
                    Details = o.OrderDetails.Select(d => new OrderDetailResponse
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.Name ?? "",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                };
            }).ToList();
        }

        public async Task<List<OrderResponse>> GetMyPaidOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAndPaidAsync(userId);
            return orders.Select(o =>
            {
                var payment = SelectPrimaryPayment(o.Payments);
                return new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CustomerName = o.User?.FullName ?? "Ẩn danh",
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.OrderDetails.Sum(d => d.Price * d.Quantity),
                    PaymentStatus = payment?.Status ?? "UNKNOWN",
                    Provider = payment?.Provider, // 🆕
                    Details = o.OrderDetails.Select(d => new OrderDetailResponse
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.Name ?? "",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                };
            }).ToList();
        }

        public async Task<List<OrderResponse>> GetMyUnpaidOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAndUnpaidAsync(userId);
            return orders.Select(o =>
            {
                var payment = SelectPrimaryPayment(o.Payments);
                return new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CustomerName = o.User?.FullName ?? "Ẩn danh",
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.OrderDetails.Sum(d => d.Price * d.Quantity),
                    PaymentStatus = payment?.Status ?? "UNKNOWN",
                    Provider = payment?.Provider, // 🆕
                    Details = o.OrderDetails.Select(d => new OrderDetailResponse
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.Name ?? "",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                };
            }).ToList();
        }
        public async Task<OrderResponse> CancelMyOrderAsync(int orderId, int currentUserId)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            // Chỉ cho hủy đơn của chính mình
            if (order.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only cancel your own order.");

            // Nếu đã thanh toán -> không cho hủy
            if (order.Payments.Any(p => p.Status.Equals("PAID", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Cannot cancel an order that has already been paid.");

            // Idempotent: nếu đã CANCELLED thì trả về luôn
            if (order.Status.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                var paymentSnap = order.Payments
                    .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)
                    .ThenByDescending(p => p.CreatedAt)
                    .FirstOrDefault();

                return new OrderResponse
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    CustomerName = order.User?.FullName ?? "Ẩn danh",
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    TotalAmount = order.OrderDetails.Sum(d => d.Price * d.Quantity),
                    PaymentStatus = paymentSnap?.Status ?? "UNKNOWN",
                    Provider = paymentSnap?.Provider,
                    Details = order.OrderDetails.Select(d => new OrderDetailResponse
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.Name ?? "",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                };
            }

            // Đổi trạng thái đơn
            order.Status = "CANCELLED";

            // Update payment: UNPAID -> CANCELLED (nếu muốn)
            foreach (var pay in order.Payments)
            {
                if (pay.Status.Equals("UNPAID", StringComparison.OrdinalIgnoreCase))
                    pay.Status = "CANCELLED";
            }

            // Restock: cộng trả tồn kho cho các sản phẩm của đơn (chỉ khi chưa thanh toán)
            foreach (var d in order.OrderDetails)
            {
                var prod = d.Product;
                if (prod != null)
                {
                    prod.Stock += d.Quantity;
                    if (prod.Stock > 0) prod.IsActive = true;
                    await _unitOfWork.Products.UpdateProductAsync(prod);
                }
            }

            await _unitOfWork.SaveAsync();

            var payment = order.Payments
                .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)
                .ThenByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User?.FullName ?? "Ẩn danh",
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.OrderDetails.Sum(d => d.Price * d.Quantity),
                PaymentStatus = payment?.Status ?? "UNKNOWN",
                Provider = payment?.Provider,
                Details = order.OrderDetails.Select(d => new OrderDetailResponse
                {
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "",
                    Price = d.Price,
                    Quantity = d.Quantity
                }).ToList()
            };
        }
        public async Task<List<OrderResponse>> GetMyCancelledOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAndCancelledAsync(userId);

            return orders.Select(o =>
            {
                var payment = SelectPrimaryPayment(o.Payments);
                return new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CustomerName = o.User?.FullName ?? "Ẩn danh",
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.OrderDetails.Sum(d => d.Price * d.Quantity),
                    PaymentStatus = payment?.Status ?? "UNKNOWN",
                    Provider = payment?.Provider,
                    Details = o.OrderDetails.Select(d => new OrderDetailResponse
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.Name ?? "",
                        Price = d.Price,
                        Quantity = d.Quantity
                    }).ToList()
                };
            }).ToList();
        }

    }
}
