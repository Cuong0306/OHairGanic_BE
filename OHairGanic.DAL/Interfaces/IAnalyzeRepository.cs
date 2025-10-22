using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Interfaces
{
    public interface IAnalyzeRepository
    {
        Task<List<Analysis?>> GetAllAnalysisAsync();
        Task<Analysis?> GetAnalyzeByIdAsync(int analyzeId);
        Task<bool> AddAnalyzeAsync(Analysis analyze);
        Task<bool> DeleteAnalyzeAsync(int analyzeId);
    }
}
