using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string UsernameOrEmail { get; set; } = string.Empty;

        // Alternatif field adları için (frontend uyumluluğu)
        public string? Username { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        public string Password { get; set; } = string.Empty;

        // UsernameOrEmail boşsa, Username veya Email'den birini kullan
        public string GetUsernameOrEmail()
        {
            if (!string.IsNullOrWhiteSpace(UsernameOrEmail))
                return UsernameOrEmail;
            
            if (!string.IsNullOrWhiteSpace(Username))
                return Username;
            
            if (!string.IsNullOrWhiteSpace(Email))
                return Email;
            
            return string.Empty;
        }

        // Custom validation
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(GetUsernameOrEmail()) && 
                   !string.IsNullOrWhiteSpace(Password);
        }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        public string NewPassword { get; set; } = string.Empty;
    }
} 