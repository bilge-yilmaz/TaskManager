using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Data;
using TaskManager.DTOs;
using TaskManager.Models;
using BCrypt.Net;

namespace TaskManager.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(MongoDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Kullanıcı adı kontrolü
            var existingUserByUsername = await _context.Users
                .Find(u => u.Username == registerDto.Username)
                .FirstOrDefaultAsync();

            if (existingUserByUsername != null)
            {
                throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor.");
            }

            // E-posta kontrolü
            var existingUserByEmail = await _context.Users
                .Find(u => u.Email == registerDto.Email)
                .FirstOrDefaultAsync();

            if (existingUserByEmail != null)
            {
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            // Yeni kullanıcı oluştur
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Users.InsertOneAsync(user);

            // JWT token oluştur
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Esnek field kontrolü
                var usernameOrEmail = loginDto.GetUsernameOrEmail();
                
                // Input validation
                if (string.IsNullOrWhiteSpace(usernameOrEmail))
                {
                    throw new ArgumentException("Kullanıcı adı veya e-posta boş olamaz.");
                }

                if (string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    throw new ArgumentException("Şifre boş olamaz.");
                }

                // Kullanıcıyı bul (username veya email ile)
                var user = await _context.Users
                    .Find(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new UnauthorizedAccessException($"'{usernameOrEmail}' kullanıcı adı/e-posta ile kullanıcı bulunamadı.");
                }

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Kullanıcı hesabı aktif değil.");
                }

                // Şifre kontrolü
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Şifre yanlış.");
                }

                // JWT token oluştur
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

                return new AuthResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is ArgumentException))
            {
                throw new Exception($"Login işlemi sırasında beklenmeyen hata: {ex.Message}", ex);
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Mevcut şifre kontrolü
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Mevcut şifre yanlış.");
            }

            // Yeni şifre hash'le ve güncelle
            var newPasswordHash = HashPassword(changePasswordDto.NewPassword);
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, newPasswordHash)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Find(u => u.Id == userId && u.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Find(u => u.Username == username && u.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Find(u => u.Email == email && u.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<long> GetUserCountAsync()
        {
            return await _context.Users.CountDocumentsAsync(u => u.IsActive);
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
} 