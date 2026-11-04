using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.DTOs;

namespace StajSistemi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtService _jwtService;

        // Dependency Injection
        public AuthService(AppDbContext context, IPasswordHasher passwordHasher, JwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<LoginResult> LoginAsync(string usernameOrEmail, string password, string ipAddress)
        {
            // 1. Kullanıcıyı bul (Kullanıcı adı VEYA e-posta ile)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail);

            // 2. Kullanıcı yoksa
            if (user == null)
            {
                await LogAttemptAsync(usernameOrEmail, ipAddress, false);
                return new LoginResult { Status = LoginStatus.InvalidCredentials };
            }

            // 3. Kullanıcı kilitliyse (Doğru şifre girse bile reddedilecek)
            if (user.IsLocked)
            {
                await LogAttemptAsync(user.UserName, ipAddress, false);
                return new LoginResult { Status = LoginStatus.Locked };
            }

            // 4. Şifre yanlışsa
            if (!_passwordHasher.Verify(password, user.PasswordHash))
            {
                user.RegisterFailedLogin(); // User içindeki kapsülleme metodumuz çalıştı (sayaç artar, 5 olursa kilitler)
                await LogAttemptAsync(user.UserName, ipAddress, false);
                await _context.SaveChangesAsync();

                return new LoginResult { Status = user.IsLocked ? LoginStatus.Locked : LoginStatus.InvalidCredentials };
            }

            // 5. Şifre doğruysa (Giriş Başarılı)
            user.ResetFailedLogins();
            await LogAttemptAsync(user.UserName, ipAddress, true);
            await _context.SaveChangesAsync();

            string roleClaim = user.Role == RoleType.Admin ? "ROLE_ADMIN" : "ROLE_STUDENT";

            // JWT üretimi
            var tokenData = _jwtService.GenerateToken(
                userId: user.Id.ToString(),
                email: user.UserName, // DÜZELTME: sözleşmede ClaimTypes.Name kullanıcı adıdır 
                role: roleClaim,
                fullName: user.UserName
            );

            return new LoginResult { Status = LoginStatus.Success, TokenData = tokenData };
        }

        // LoginAttempts tablosuna log yazan yardımcı metot 
        private async Task LogAttemptAsync(string username, string ip, bool success)
        {
            var attempt = new LoginAttempt
            {
                UserNameOrEmail = username,
                IpAddress = ip,
                AttemptedAt = DateTime.UtcNow,
                Success = success
            };

            _context.LoginAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }
    }
}