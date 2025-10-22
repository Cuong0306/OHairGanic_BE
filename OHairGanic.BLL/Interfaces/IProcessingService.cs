using Microsoft.AspNetCore.Http;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface IProcessingService
    {
        Task<AnalyzeResponse> AnalyzeNewImageAsync(IFormFile imageFile, int userId, string angle);
    }
}
