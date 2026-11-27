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

        // Resmi tatil tablosuna 2026-2027 tatillerini ekler (Ramazan ve Kurban arifeleri dahil).
        // Tabloda zaten olan tarihlere dokunmaz, tekrar çalıştırmak güvenlidir.
        public static async Task SeedHolidaysAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existing = (await context.Holidays.Select(h => h.Date).ToListAsync())
                .Select(d => d.Date)
                .ToHashSet();

            var added = 0;
            foreach (var (date, name) in DefaultHolidays)
            {
                if (existing.Contains(date)) continue;
                context.Holidays.Add(new Holiday { Date = date, Name = name });
                added++;
            }

            if (added > 0)
                await context.SaveChangesAsync();
        }

        private static readonly (DateTime Date, string Name)[] DefaultHolidays =
        [
            (new DateTime(2026, 1, 1), "Yılbaşı"),
            (new DateTime(2026, 3, 19), "Ramazan Bayramı Arifesi"),
            (new DateTime(2026, 3, 20), "Ramazan Bayramı 1. Gün"),
            (new DateTime(2026, 3, 21), "Ramazan Bayramı 2. Gün"),
            (new DateTime(2026, 3, 22), "Ramazan Bayramı 3. Gün"),
            (new DateTime(2026, 4, 23), "Ulusal Egemenlik ve Çocuk Bayramı"),
            (new DateTime(2026, 5, 1), "Emek ve Dayanışma Günü"),
            (new DateTime(2026, 5, 19), "Atatürk'ü Anma, Gençlik ve Spor Bayramı"),
            (new DateTime(2026, 5, 26), "Kurban Bayramı Arifesi"),
            (new DateTime(2026, 5, 27), "Kurban Bayramı 1. Gün"),
            (new DateTime(2026, 5, 28), "Kurban Bayramı 2. Gün"),
            (new DateTime(2026, 5, 29), "Kurban Bayramı 3. Gün"),
            (new DateTime(2026, 5, 30), "Kurban Bayramı 4. Gün"),
            (new DateTime(2026, 7, 15), "Demokrasi ve Milli Birlik Günü"),
            (new DateTime(2026, 8, 30), "Zafer Bayramı"),
            (new DateTime(2026, 10, 29), "Cumhuriyet Bayramı"),
            (new DateTime(2027, 1, 1), "Yılbaşı"),
            (new DateTime(2027, 3, 8), "Ramazan Bayramı Arifesi"),
            (new DateTime(2027, 3, 9), "Ramazan Bayramı 1. Gün"),
            (new DateTime(2027, 3, 10), "Ramazan Bayramı 2. Gün"),
            (new DateTime(2027, 3, 11), "Ramazan Bayramı 3. Gün"),
            (new DateTime(2027, 4, 23), "Ulusal Egemenlik ve Çocuk Bayramı"),
            (new DateTime(2027, 5, 1), "Emek ve Dayanışma Günü"),
            (new DateTime(2027, 5, 15), "Kurban Bayramı Arifesi"),
            (new DateTime(2027, 5, 16), "Kurban Bayramı 1. Gün"),
            (new DateTime(2027, 5, 17), "Kurban Bayramı 2. Gün"),
            (new DateTime(2027, 5, 18), "Kurban Bayramı 3. Gün"),
            (new DateTime(2027, 5, 19), "Kurban Bayramı 4. Gün / Atatürk'ü Anma, Gençlik ve Spor Bayramı"),
            (new DateTime(2027, 7, 15), "Demokrasi ve Milli Birlik Günü"),
            (new DateTime(2027, 8, 30), "Zafer Bayramı"),
            (new DateTime(2027, 10, 29), "Cumhuriyet Bayramı"),
        ];
    }
}