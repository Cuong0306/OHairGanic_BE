using OHairGanic.DAL.Models;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface IUserService
    {
        Task<bool> UpdateUserAsync(UpdateUserRequest dto);
        Task<bool> SoftDeleteUserAsync(int userId);
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<User> GetByIdAsync(int id);
        Task<bool> HardDeleteUserAsync(int id);
    }
}
