using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;
using System.Security.Claims;

namespace OHairGanic.API.Controllers
{
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost(ApiRoutes.Order.Create)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest dto)
        {
            // Model binding đã tự 400 khi body null/invalid do [ApiController],
            // nhưng mình vẫn bảo vệ thêm các case đặc biệt.
            if (dto == null) return BadRequest(new { message = "Request body is required." });

            var userIdString =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("nameid")?.Value ??
                User.FindFirst("sub")?.Value ??
                User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "User not authenticated (no valid ID claim found)." });

            try
            {
                var result = await _orderService.CreateOrderAsync(dto, userId);
                // Có thể trả 201 nếu muốn:
                // return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)         // ví dụ: "Product(s) not found: 2"
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)            // ví dụ: quantity <= 0, items rỗng
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)    // ví dụ: inactive / hết tồn kho
            {
                // tuỳ bạn: Conflict(409) hoặc BadRequest(400)
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // tránh lộ stack trace ở production
                return Problem(title: "Internal Server Error", detail: ex.Message, statusCode: 500);
            }
        }

        [Authorize]
        [HttpGet(ApiRoutes.Order.GetById)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _orderService.GetOrderByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet(ApiRoutes.Order.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPut(ApiRoutes.Order.AdminUpdateStatus)]
        public async Task<IActionResult> AdminUpdateStatus([FromBody] AdminUpdateStatusRequest dto)
        {
            try
            {
                var result = await _orderService.AdminUpdateStatusAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(title: "Update Failed", detail: ex.Message, statusCode: 400);
            }
        }
        [Authorize]
        [HttpGet(ApiRoutes.Order.GetMine)]
        public async Task<IActionResult> GetMine()
        {
            var userIdStr =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("nameid")?.Value ??
                User.FindFirst("sub")?.Value ??
                User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            var result = await _orderService.GetOrdersByUserIdAsync(currentUserId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet(ApiRoutes.Order.GetByUserId)]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("nameid")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            // FIX: đọc role từ cả ClaimTypes.Role và "role"
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                       ?? User.FindFirst("role")?.Value
                       ?? "";
            var isAdmin = string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && currentUserId != userId)
                return Forbid();

            var result = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(result);
        }
        [Authorize]
        [HttpGet(ApiRoutes.Order.GetMyPaid)]
        public async Task<IActionResult> GetMyPaid()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("nameid")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            var result = await _orderService.GetMyPaidOrdersAsync(currentUserId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet(ApiRoutes.Order.GetMyUnpaid)]
        public async Task<IActionResult> GetMyUnpaid()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("nameid")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            var result = await _orderService.GetMyUnpaidOrdersAsync(currentUserId);
            return Ok(result);
        }
        [Authorize]
        [HttpPut(ApiRoutes.Order.CancelMine)]
        public async Task<IActionResult> CancelMine(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("nameid")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            try
            {
                var result = await _orderService.CancelMyOrderAsync(id, currentUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                // Ví dụ: đã thanh toán thì không hủy được
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return Problem(title: "Cancel Failed", detail: ex.Message, statusCode: 500);
            }
        }
        [Authorize]
        [HttpGet(ApiRoutes.Order.GetMyCancelled)]
        public async Task<IActionResult> GetMyCancelled()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("nameid")?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var currentUserId))
                return Unauthorized(new { message = "User not authenticated." });

            var result = await _orderService.GetMyCancelledOrdersAsync(currentUserId);
            return Ok(result);
        }

    }
}
