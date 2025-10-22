using Microsoft.IdentityModel.Tokens;
using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Config;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace OHairGanic.BLL.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork)
        {
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // LOGIN
        // ============================================================
        public async Task<LoginResponse?> LoginAsync(LoginRequest dto)
        {
            // ---------- VALIDATION ----------
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Login request cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.", nameof(dto.Email));

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required.", nameof(dto.Password));

            if (!IsValidEmail(dto.Email))
                throw new ArgumentException("Invalid email format.", nameof(dto.Email));

            // ---------- GET USER ----------
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email.Trim().ToLower());
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            // ---------- ACCOUNT STATUS CHECK ----------
            if (user.Status?.ToLower() == "inactive")
                throw new UnauthorizedAccessException("Account is inactive. Please contact support.");

            if (user.Status?.ToLower() == "locked")
                throw new UnauthorizedAccessException("Account is locked due to multiple failed attempts.");

            // ---------- PASSWORD VERIFY ----------
            bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid email or password.");

            // ---------- BUILD CLAIMS ----------
            var claims = new List<Claim>
            {
                new Claim("nameid", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("role", user.Role ?? "User")
            };

            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                IssuedAt = now,
                NotBefore = now,
                Expires = now.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            // ---------- RESPONSE ----------
            return new LoginResponse
            {
                Token = jwt,
                ExpiresIn = _jwtSettings.ExpiryMinutes * 60,
                //Role = user.Role ?? "User"
            };
        }

        // ============================================================
        // REGISTER
        // ============================================================
        public async Task<RegisterResponse?> RegisterAsync(RegisterRequest dto)
        {
            // ---------- VALIDATION ----------
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Register request cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.", nameof(dto.Email));

            if (!IsValidEmail(dto.Email))
                throw new ArgumentException("Invalid email format.", nameof(dto.Email));

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required.", nameof(dto.Password));

            if (dto.Password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");

            //if (!HasStrongPassword(dto.Password))
            //    throw new ArgumentException("Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character.");

            // Normalize input
            var email = dto.Email.Trim().ToLower();

            // ---------- DUPLICATE EMAIL ----------
            if (await _unitOfWork.Users.IsEmailExistsAsync(email, 0))
                throw new InvalidOperationException("Email already exists.");

            // ---------- ROLE / STATUS ----------
            //var role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role.Trim();
            //var status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim();

            // ---------- PASSWORD HASH ----------
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

            // ---------- CREATE USER ----------
            var user = new User
            {
                Email = email,
                PasswordHash = hashedPassword,
                FullName = dto.FullName?.Trim() ?? "Unnamed User",
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Role = "User",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            bool result = await _unitOfWork.Users.CreateAsync(user);
            if (!result)
                throw new Exception("Failed to create user due to database error.");

            // ---------- RESPONSE ----------
            return new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Status = user.Status
            };
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromMilliseconds(250));
            }
            catch
            {
                return false;
            }
        }

        private static bool HasStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
    }
}
