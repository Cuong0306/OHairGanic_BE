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
    }
}
