using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;
using System.Security.Claims;

namespace OHairGanic.API.Controllers
{
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IAnalyzeService _analyzeService;
        private readonly IHairAnalysisService _hairService;
        private readonly ICaptureService _captureService;

        public AnalyzeController(
            IAnalyzeService analyzeService,
            IHairAnalysisService hairService,
            ICaptureService captureService)
        {
            _analyzeService = analyzeService;
            _hairService = hairService;
            _captureService = captureService;
        }

        // ✅ POST: api/analyzes/by-url
        [Authorize]
        [HttpPost(ApiRoutes.Analyze.AnalyzeByUrl)]
        public async Task<IActionResult> AnalyzeFromUrlAndSaveAsync([FromBody] AnalyzeByUrlRequest dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.ImageUrl))
                    return BadRequest(new { message = "ImageUrl is required." });

                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                       ?? throw new Exception("Invalid userId in token"));

                var captureRes = await _captureService.AddCaptureAsync(userId, new CreateCaptureRequest
                {
                    Angle = dto.Angle,
                    ImageUrl = dto.ImageUrl
                });

                var ai = await _hairService.AnalyzeFromUrlAsync(dto.ImageUrl, dto.Angle);

                var saved = await _analyzeService.AddAnalyzeAsync(new CreateAnalyzeRequest
                {
                    CaptureId = captureRes.Id,
                    Oiliness = ai.Oiliness,
                    Dryness = ai.Dryness,
                    DandruffScore = ai.DandruffScore,
                    Label = ai.Label,
                    ModelVersion = (_hairService as OnnxHairService)?.ModelVersion ?? "unknown"
                });

                return Ok(saved);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/analyzes/daily
        [Authorize]
        [HttpGet(ApiRoutes.Analyze.GetDaily)]
        public async Task<IActionResult> GetDailyAnalysesAsync()
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                       ?? throw new Exception("Invalid userId in token"));

                var result = await _analyzeService.GetDailyAnalysesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET: api/analyzes (Admin)
        [Authorize]
        [HttpGet(ApiRoutes.Analyze.GetAll)]
        public async Task<IActionResult> GetAllAsync()
        {
            var list = await _analyzeService.GetAllAnalysesAsync();
            return Ok(list);
        }

        // ✅ GET: api/analyzes/{id}
        [Authorize]
        [HttpGet(ApiRoutes.Analyze.GetById)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _analyzeService.GetAnalyzeByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"No analysis found with id = {id}" });

            return Ok(result);
        }

        // ✅ GET: api/analyzes/by-user/{userId}
        [Authorize]
        [HttpGet(ApiRoutes.Analyze.GetByUserId)]
        public async Task<IActionResult> GetByUserIdAsync(int userId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Admin" && currentUserId != userId)
                return Forbid("You can only view your own analyses.");

            var result = await _analyzeService.GetAnalysesByUserIdAsync(userId);
            return Ok(result);
        }

        // ✅ GET: /api/analyzes/filter?from=2025-10&to=2025-11&userId=3
        // ✅ GET: /api/analyzes/filter?from=2025&to=2025-03
        // ✅ GET: /api/analyzes/filter?from=2025-10-01&to=2025-10-24
        // ✅ GET: /api/analyzes/filter  (không tham số -> ALL của user hiện tại)
        [Authorize]
        [HttpGet(ApiRoutes.Analyze.Filter)]
        public async Task<IActionResult> Filter(
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] int? userId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var role = User.FindFirstValue(ClaimTypes.Role);
                bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

                var result = await _analyzeService.FilterAnalysesAsync(
                    targetUserId: userId,
                    from: from,
                    to: to,
                    isAdmin: isAdmin,
                    currentUserId: currentUserId
                );

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
