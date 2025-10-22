using OHairGanic.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        ICaptureRepository Captures { get; }
        IAnalyzeRepository Analyzes { get; }
        IOrderRepository Orders { get; }
        IPaymentRepository Payments { get; }
        Task<int> SaveAsync();
    }
}
