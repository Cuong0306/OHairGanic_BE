using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OHairGanicDBContext _context;

        public IUserRepository Users { get; }
        public IProductRepository Products { get; }
        public ICaptureRepository Captures { get; }
        public IAnalyzeRepository Analyzes { get; }
        public IOrderRepository Orders { get; }
        public IPaymentRepository Payments { get; }
        public UnitOfWork(OHairGanicDBContext context,
            IUserRepository userRepository,
            IProductRepository productRepository,
            ICaptureRepository capturesRepository,
            IAnalyzeRepository analyzesRepository,
            IOrderRepository orders,
            IPaymentRepository payments)
        {
            _context = context;
            Users = userRepository;
            Products = productRepository;
            Captures = capturesRepository;
            Analyzes = analyzesRepository;
            Orders = orders;
            Payments = payments;
        }
        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
