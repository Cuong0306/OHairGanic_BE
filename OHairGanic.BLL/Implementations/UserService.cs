using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // ================== ME APIs (user tự thao tác) ==================

        // Lấy hồ sơ chính mình (controller sẽ lấy userId từ JWT rồi gọi hàm này)
        public async Task<GetUserByIdResponse> GetMeAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || string.Equals(user.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
                throw new Exception("User not found");

            return Map(user);
        }

        // Cập nhật hồ sơ chính mình — KHÔNG cho đổi Role/Status
        public async Task<GetUserByIdResponse> UpdateMeAsync(int userId, UpdateMeRequest dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || string.Equals(user.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
                throw new Exception("User not found");

            // Cho phép đổi Email (nếu bạn muốn), nhớ check trùng
            /*if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !dto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _unitOfWork.Users.IsEmailExistsAsync(dto.Email!, user.Id);
                if (exists) throw new Exception("Email already in use");
                user.Email = dto.Email!.Trim();
            }*/

            // Cập nhật thông tin cá nhân
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber.Trim();

            // ❌ KHÔNG cập nhật Role/Status ở đây (dù FE có gửi cũng bỏ qua)

            var ok = await _unitOfWork.Users.UpdateUserAsync(user);
            if (!ok) throw new Exception("Failed to update profile");
            return Map(user);
        }

        // ================== ADMIN/STAFF APIs (giữ nguyên code của bạn) ==================

        public async Task<bool> UpdateUserAsync(UpdateUserRequest dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(dto.Id);
            if (user == null) throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !dto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _unitOfWork.Users.IsEmailExistsAsync(dto.Email!, dto.Id);
                if (exists) throw new Exception("Email already in use");
            }

            if (dto.Email != null) user.Email = dto.Email;
            if (dto.FullName != null) user.FullName = dto.FullName;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.Status != null) user.Status = dto.Status; // admin được phép
            if (dto.Role != null) user.Role = dto.Role;       // admin được phép

            return await _unitOfWork.Users.UpdateUserAsync(user);
        }

        public async Task<bool> SoftDeleteUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Status == "Deleted") return false;

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

        public async Task<GetUserByIdResponse> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || user.Status == "Deleted")
                throw new Exception("User not found");

            return new GetUserByIdResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Status = user.Status
            };
        }

        public async Task<bool> HardDeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) throw new Exception("User not found");

            if (user.Status != "Deleted")
                throw new Exception("This User cannot be Hard deleted");

            var result = await _unitOfWork.Users.HardDeleteUserAsync(user.Id);
            if (result)
            {
                await _unitOfWork.SaveAsync();
                return true;
            }
            throw new Exception("Failed to delete user");
        }

        // ================== Helper ==================
        private static GetUserByIdResponse Map(User u) => new()
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FullName = u.FullName ?? string.Empty,
            CreatedAt = u.CreatedAt,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role ?? string.Empty,
            Status = u.Status ?? string.Empty
        };
    }
}
