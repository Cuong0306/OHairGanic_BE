using Microsoft.EntityFrameworkCore;
using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OHairGanic.DTO.Constants.ApiRoutes;
using Capture = OHairGanic.DAL.Models.Capture;


namespace OHairGanic.DAL.Implementations
{
    public class CaptureRepository : ICaptureRepository
    {
        private readonly OHairGanicDBContext _context;
        public CaptureRepository(OHairGanicDBContext context)
        {
            _context = context;
        }
        public async Task<bool> AddCaptureAsync(Capture capture)
        {
            _context.Captures.Add(capture);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCaptureAsync(int captuteId)
        {
            var capture = await _context.Captures.FindAsync(captuteId);
            if (capture != null)
            {
                _context.Captures.Remove(capture);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<Capture?>> GetAllCapturesAsync()
        {
            return await _context.Captures
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<Capture?> GetCaptureByIdAsync(int captuteId)
        {
            return await _context.Captures
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == captuteId);
        }
    }
}
