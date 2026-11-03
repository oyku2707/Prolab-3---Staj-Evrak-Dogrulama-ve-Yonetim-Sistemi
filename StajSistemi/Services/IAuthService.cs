using StajSistemi.DTOs;

namespace StajSistemi.Services
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(string usernameOrEmail, string password, string ipAddress);
    }
}