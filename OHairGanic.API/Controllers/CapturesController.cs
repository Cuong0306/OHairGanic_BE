using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;

namespace OHairGanic.API.Controllers
{
    
    [ApiController]
    
    
    public class CapturesController : ControllerBase
    {
        private readonly ICaptureService _captureService;

        public CapturesController(ICaptureService captureService)
        {
            _captureService = captureService;
        }

        // POST: api/captures
        [Authorize]
        [HttpPost(ApiRoutes.Capture.Create)]
        
        public async Task<IActionResult> AddCapture([FromBody] CreateCaptureRequest dto)
        {
            var userIdString = User.FindFirst("nameid")?.Value 
                   ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("User not authenticated");

            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Invalid user ID format");

            var result = await _captureService.AddCaptureAsync(userId, dto);
            return Ok(result);
        }


        // GET: api/captures
        [Authorize]
        [HttpGet(ApiRoutes.Capture.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var captures = await _captureService.GetAllCapturesAsync();
            return Ok(captures);
        }

        // GET: api/captures/{id}
        [Authorize]
        [HttpGet(ApiRoutes.Capture.GetById)]
        public async Task<IActionResult> GetById(int id)
        {
            var capture = await _captureService.GetCaptureByIdAsync(id);
            if (capture == null)
                return NotFound("Capture not found");
            return Ok(capture);
        }

        // DELETE: api/captures/{id}
        [Authorize]
        [HttpDelete(ApiRoutes.Capture.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _captureService.DeleteCaptureAsync(id);
            if (!success)
                return BadRequest("Failed to delete capture");
            return Ok(new { message = "Capture deleted successfully" });
        }
    }
}
