using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Interfaces
{
    public interface ICaptureRepository
    {
        Task<List<Capture?>> GetAllCapturesAsync();
        Task<Capture?> GetCaptureByIdAsync(int captuteId);
        Task<bool> AddCaptureAsync(Capture captute);
        Task<bool> DeleteCaptureAsync(int captuteId);
    }
}
