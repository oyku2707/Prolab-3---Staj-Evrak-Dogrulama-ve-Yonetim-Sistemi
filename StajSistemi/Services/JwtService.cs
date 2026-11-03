using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StajSistemi.DTOs;

namespace StajSistemi.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResponseDto GenerateToken(string userId, string email, string role, string fullName)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key tanımlı değil!"));

            // Ekip sözleşmesine uygun Claim tipleri:
            // 1. ClaimTypes.NameIdentifier -> GetUserId() metodunun okuduğu anahtar
            // 2. ClaimTypes.Name -> Kullanıcı e-posta / adı
            // 3. ClaimTypes.Role -> ROLE_ADMIN veya ROLE_STUDENT
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", fullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiration = DateTime.UtcNow.AddHours(4); // Token 4 saat geçerli

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return new TokenResponseDto
            {
                Token = tokenString,
                Expiration = expiration,
                Role = role,
                FullName = fullName
            };
        }
    }
}