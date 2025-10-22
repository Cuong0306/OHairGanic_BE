using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest dto);
        Task<RegisterResponse?> RegisterAsync(RegisterRequest dto);
    }
}
