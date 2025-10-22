using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface ICaptureService
    {
        Task<List<CaptureResponse>> GetAllCapturesAsync();
        Task<CaptureResponse> GetCaptureByIdAsync(int id);
        Task<CaptureResponse> AddCaptureAsync(int userId, CreateCaptureRequest dto);
        Task<bool> DeleteCaptureAsync(int id);
    }
}
