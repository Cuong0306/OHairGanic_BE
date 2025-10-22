using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Requests;
using System.Threading.Tasks;

namespace OHairGanic.API.Controllers
{
    [ApiController]
    [Route("api/process")]
    public class ProcessingController : ControllerBase
    {
        private readonly IHairAnalysisService _hairService;

        public ProcessingController(IHairAnalysisService hairService)
        {
            _hairService = hairService;
        }

        /// <summary>
        /// Phân tích tóc từ ảnh đã upload (qua URL Cloudinary)
        /// </summary>
        [Authorize]
        [HttpPost("analyze")]
        [Consumes("application/json")]
        public async Task<IActionResult> AnalyzeHair([FromBody] AnalyzeByUrlRequest dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ImageUrl))
                return BadRequest("Thiếu URL ảnh. Hãy gửi URL Cloudinary hợp lệ.");

            try
            {
                var result = await _hairService.AnalyzeFromUrlAsync(dto.ImageUrl, dto.Angle);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xử lý ảnh: {ex.Message}");
            }
        }
    }
}
