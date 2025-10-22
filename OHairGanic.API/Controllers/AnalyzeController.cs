using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DTO.Constants;
using OHairGanic.DTO.Requests;

namespace OHairGanic.API.Controllers
{
    
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IAnalyzeService _analyzeService;
        public AnalyzeController(IAnalyzeService analyzeService)
        {
            _analyzeService = analyzeService;
        }
        [Authorize]
        [HttpPost(ApiRoutes.Analyze.Create)]
        public async Task<IActionResult> AddAnalyzeAsync([FromBody] CreateAnalyzeRequest dto)
        {
            try
            {
                var result = await _analyzeService.AddAnalyzeAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet(ApiRoutes.Analyze.GetById)]
        public async Task<IActionResult> GetAnalyzeByIdAsync(int id)
        {
            try
            {
                var result = await _analyzeService.GetAnalyzeByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
