using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OhairGanicContext _context;
        public void Dispose()
        {
            _context.Dispose();
        }
        public UnitOfWork(OhairGanicContext context)
        {
            _context = context;
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
