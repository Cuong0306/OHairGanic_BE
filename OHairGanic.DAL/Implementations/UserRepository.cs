using Microsoft.EntityFrameworkCore;
using OHairGanic.DAL.Interfaces;
using OHairGanic.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.DAL.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly OHairGanicDBContext _context;
        public UserRepository(OHairGanicDBContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<User>> GetAllActiveUsersAsync()
        {
            return await _context.Users
                .Where(u => u.Status != "Deleted")
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> HardDeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsEmailExistsAsync(string email, int userId)
        {
            var norm = email.ToLower();
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == norm && u.Id != userId);
        }

        public async Task<bool> UpdateUserAsync(User dto)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (entity == null) return false;

            if (dto.Email != null) entity.Email = dto.Email;
            if (dto.FullName != null) entity.FullName = dto.FullName;
            if (dto.PhoneNumber != null) entity.PhoneNumber = dto.PhoneNumber;
            if (dto.Role != null) entity.Role = dto.Role;
            if (dto.Status != null) entity.Status = dto.Status;

            _context.Users.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
