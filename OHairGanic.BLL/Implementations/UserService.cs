using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public class UserService : IUserService
    {

        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(dto.Id);
            if (user == null) throw new Exception("User not found");

            // Nếu client có gửi email và khác email hiện tại thì mới check trùng
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !dto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _unitOfWork.Users.IsEmailExistsAsync(dto.Email!, dto.Id);
                if (exists) throw new Exception("Email already in use");
            }

            // Chỉ gán những field được gửi (khác null)
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.Status != null) user.Status = dto.Status;
            if (dto.Role != null) user.Role = dto.Role;

            return await _unitOfWork.Users.UpdateUserAsync(user);
        }

        public async Task<bool> SoftDeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.Status == "Deleted")
                return false;

            user.Status = "Deleted";
            user.Email = $"deleted_{Guid.NewGuid()}@deleted.com";
            user.FullName = "Deleted User";

            user.PasswordHash = $"{Guid.NewGuid()}";

            await _unitOfWork.Users.UpdateUserAsync(user);
            return true;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllActiveUsersAsync();

            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                CreatedAt = u.CreatedAt,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                Status = u.Status
            }).ToList();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.Status == "Deleted")
                throw new Exception("User not found");

            return user;
        }

        public async Task<bool> HardDeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user != null)
            {
                var userId = user.Id;

                if (user.Status != "Deleted")
                    throw new Exception("This User cannot be Hard deleted");

                var result = await _unitOfWork.Users.HardDeleteUserAsync(userId);
                if (result)
                {
                    await _unitOfWork.SaveAsync();
                    return true;
                }
                else
                {
                    throw new Exception("Failed to delete user");
                }
            }
            else
            {
                throw new Exception("User not found");
            }

        }
    }
}
