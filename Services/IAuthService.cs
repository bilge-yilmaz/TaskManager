using TaskManager.DTOs;
using TaskManager.Models;

namespace TaskManager.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<long> GetUserCountAsync();
        string GenerateJwtToken(User user);
        bool VerifyPassword(string password, string hash);
        string HashPassword(string password);
    }
} 