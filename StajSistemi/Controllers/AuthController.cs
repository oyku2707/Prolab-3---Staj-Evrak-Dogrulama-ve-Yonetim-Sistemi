using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.DTOs;
using StajSistemi.Services;
using System.Security.Claims;

namespace StajSistemi.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Zaten giriş yapmış biri varsa doğrudan paneline yönlendir
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("ROLE_ADMIN"))
                    return RedirectToAction("Index", "Admin");
                return RedirectToAction("Index", "Student");
            }
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Lütfen tüm alanları doldurunuz.");
                return View();
            }

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _authService.LoginAsync(usernameOrEmail, password, ipAddress);

            if (result.Status == LoginStatus.Locked)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız çok fazla hatalı giriş nedeniyle kilitlenmiştir. Lütfen yönetici ile iletişime geçin.");
                return View();
            }

            if (result.Status == LoginStatus.InvalidCredentials || result.TokenData == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                return View();
            }

            // JWT içindeki claim'leri okuyarak Cookie (Oturum) oluşturuyoruz
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(result.TokenData.Token);
            var claims = jwtToken.Claims.ToList();

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.TokenData.Expiration
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // Role göre yönlendirme
            if (result.TokenData.Role == "ROLE_ADMIN")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Student");
        }

        //Logout
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}