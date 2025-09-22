using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveAsync();
    }
}
