// OHairGanic.API/Controllers/UserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;
using System.Security.Claims;

namespace OHairGanic.API.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) => _userService = userService;

        // ====== ME APIs ======

        // GET /api/user/me
        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Users.GetMe)]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userId = RequireUserId();
                var me = await _userService.GetMeAsync(userId);
                return Ok(me);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/user/me
        // User chỉ được cập nhật thông tin cá nhân (không đổi role/status)
        [Authorize]
        [HttpPut]
        [Route(ApiRoutes.Users.UpdateMe)]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = RequireUserId();
                var me = await _userService.UpdateMeAsync(userId, dto);
                return Ok(me);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ====== ADMIN/STAFF APIs (giữ nguyên của bạn) ======

        [Authorize]
        [HttpPut]
        [Route(ApiRoutes.Users.Update)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var success = await _userService.UpdateUserAsync(dto);
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route(ApiRoutes.Users.Delete)]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Invalid user ID");
                var success = await _userService.SoftDeleteUserAsync(id);
                return success ? Ok("User deleted successfully") : NotFound("User not found or already deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route(ApiRoutes.Users.HardDelete)]
        public async Task<IActionResult> HardDeleteUser(int userId)
        {
            try
            {
                if (userId <= 0) return BadRequest("Invalid user ID");
                var success = await _userService.HardDeleteUserAsync(userId);
                return success ? Ok("User deleted successfully") : NotFound("User not found or already deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Users.GetAll)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Users.GetById)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Helper: lấy userId từ JWT
        private int RequireUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub)) throw new Exception("Invalid userId in token");
            return int.Parse(sub);
        }
    }
}
