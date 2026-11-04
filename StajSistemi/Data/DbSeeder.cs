using Microsoft.EntityFrameworkCore;
using StajSistemi.Domain.Entities;
using StajSistemi.Services;

namespace StajSistemi.Data
{
    public static class DbSeeder
    {
        // Uygulama açılırken admin yoksa ilk admin hesabını oluşturur.
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Zaten bir admin varsa hiçbir şey yapma
            if (await context.Users.AnyAsync(u => u is Admin))
                return;

            // Şifre koddan değil, user-secrets'tan okunur
            var password = config["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("UYARI: Seed:AdminPassword tanımlı değil, admin oluşturulmadı.");
                return;
            }

            var admin = new Admin
            {
                UserName = "admin",
                Email = "admin@staj.local"
            };
            admin.SetPasswordHash(hasher.Hash(password));

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}