using StajSistemi.Domain.Enums;

namespace StajSistemi.DTOs
{
    // Giriş durumlarını belirten Enum
    public enum LoginStatus
    {
        Success,
        InvalidCredentials,
        Locked
    }

    // Servisin geri döndüreceği sonuç paketi
    public class LoginResult
    {
        public LoginStatus Status { get; set; }
        public TokenResponseDto? TokenData { get; set; }
    }
}