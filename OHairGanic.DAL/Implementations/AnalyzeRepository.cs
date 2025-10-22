using Microsoft.EntityFrameworkCore;
using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Implementations
{
    public class AnalyzeRepository : IAnalyzeRepository
    {
        private readonly OHairGanicDBContext _context;
        public AnalyzeRepository(OHairGanicDBContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAnalyzeAsync(Analysis analyze)
        {
            _context.Analyses.Add(analyze);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAnalyzeAsync(int analyzeId)
        {
            var analyze = await _context.Analyses.FindAsync(analyzeId);
            if (analyze != null)
            {
                _context.Analyses.Remove(analyze);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<Analysis?>> GetAllAnalysisAsync()
        {
            return await _context.Analyses
                .Include(a => a.Capture)
                .ToListAsync();
        }

        public async Task<Analysis?> GetAnalyzeByIdAsync(int analyzeId)
        {
            return await _context.Analyses
                .Include(a => a.Capture)
                .FirstOrDefaultAsync(c => c.Id == analyzeId);
        }
    }
}
