using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.DTOs;
using TaskManager.Services;

namespace TaskManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Yeni kullanıcı kaydı
        /// </summary>
        /// <param name="registerDto">Kayıt bilgileri</param>
        /// <returns>JWT token ve kullanıcı bilgileri</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kayıt işlemi sırasında bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcı girişi
        /// </summary>
        /// <param name="loginDto">Giriş bilgileri</param>
        /// <returns>JWT token ve kullanıcı bilgileri</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Custom validation kontrolü
                if (loginDto == null || !loginDto.IsValid())
                {
                    return BadRequest(new { 
                        message = "Validation hatası", 
                        errors = new {
                            UsernameOrEmail = string.IsNullOrWhiteSpace(loginDto?.GetUsernameOrEmail()) ? 
                                new[] { "Kullanıcı adı veya e-posta gereklidir" } : null,
                            Password = string.IsNullOrWhiteSpace(loginDto?.Password) ? 
                                new[] { "Şifre gereklidir" } : null
                        },
                        receivedData = new { 
                            usernameOrEmail = loginDto?.UsernameOrEmail ?? "null",
                            username = loginDto?.Username ?? "null",
                            email = loginDto?.Email ?? "null",
                            passwordLength = loginDto?.Password?.Length ?? 0
                        }
                    });
                }

                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Giriş işlemi sırasında bir hata oluştu.", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Şifre değiştirme
        /// </summary>
        /// <param name="changePasswordDto">Şifre değiştirme bilgileri</param>
        /// <returns>İşlem sonucu</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
                if (result)
                {
                    return Ok(new { message = "Şifre başarıyla değiştirildi." });
                }
                else
                {
                    return BadRequest(new { message = "Şifre değiştirme işlemi başarısız." });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Şifre değiştirme sırasında bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcı profil bilgilerini getir
        /// </summary>
        /// <returns>Kullanıcı bilgileri</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Geçersiz token." });
                }

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Profil bilgileri alınırken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Token doğrulama
        /// </summary>
        /// <returns>Token geçerlilik durumu</returns>
        [HttpGet("validate-token")]
        [Authorize]
        public ActionResult ValidateToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            return Ok(new
            {
                valid = true,
                userId = userId,
                username = username,
                email = email
            });
        }

        /// <summary>
        /// MongoDB bağlantı testi
        /// </summary>
        /// <returns>Veritabanı bağlantı durumu</returns>
        [HttpGet("db-health")]
        public async Task<ActionResult> CheckDatabaseHealth()
        {
            try
            {
                var userCount = await _authService.GetUserCountAsync();
                return Ok(new
                {
                    status = "healthy",
                    message = "MongoDB bağlantısı başarılı",
                    userCount = userCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    message = "MongoDB bağlantısı başarısız",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
} 