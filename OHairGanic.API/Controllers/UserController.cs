using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;

namespace OHairGanic.API.Controllers
{
    
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        [HttpPut]
        [Route(ApiRoutes.Users.Update)]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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


        /// <summary>
        /// Deletes a user 
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        [Authorize]
        [HttpDelete]
        [Route(ApiRoutes.Users.Delete)]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {

            try
            {
                if (id <= 0)
                    return BadRequest("Invalid user ID");

                var success = await _userService.SoftDeleteUserAsync(id);
                return success ? Ok("User deleted successfully") : NotFound("User not found or already deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Hard deletes a user from database
        /// </summary>
        /// <param name="userId">The ID of the user to delete</param>
        /// 
        [Authorize]
        [HttpDelete]
        [Route(ApiRoutes.Users.HardDelete)]
        public async Task<IActionResult> HardDeleteUser(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest("Invalid user ID");
                var success = await _userService.HardDeleteUserAsync(userId);
                return success ? Ok("User deleted successfully") : NotFound("User not found or already deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns>List of users</returns>
        /// 
        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        [Route(ApiRoutes.Users.GetAll)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get a user by ID 
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        /// 
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
    }
}
