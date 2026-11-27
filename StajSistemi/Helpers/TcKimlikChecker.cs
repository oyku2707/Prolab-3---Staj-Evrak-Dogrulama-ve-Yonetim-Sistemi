using StajSistemi.Domain.Enums;

namespace StajSistemi.Helpers;

/// <summary>
/// T.C. / yabancı kimlik numarası kontrolü. Hem belge doğrulama kuralı (TcKimlikRule)
/// hem de veri seti yükleme (DatasetImportService) aynı mantığı buradan kullanır.
/// </summary>
public static class TcKimlikChecker
{
    /// <summary>Geçerliyse null, değilse kullanıcıya gösterilecek hata metni döner.</summary>
    public static string? Validate(string tc, Nationality nationality)
    {
        if (tc.Length != 11 || !tc.All(c => c >= '0' && c <= '9'))
            return "Kimlik numarası 11 haneli olmalı ve sadece rakamlardan oluşmalıdır.";

        if (nationality == Nationality.TC)
        {
            if (tc[0] == '0')
                return "T.C. uyruklu için kimlik numarasının ilk hanesi 0 olamaz.";

            var digits = tc.Select(c => c - '0').ToArray();
            var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
            var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
            var tenth = ((oddSum * 7 - evenSum) % 10 + 10) % 10;
            var eleventh = digits.Take(10).Sum() % 10;

            if (digits[9] != tenth || digits[10] != eleventh)
                return "Kimlik numarası geçerli bir T.C. kimlik numarası değil.";
        }
        else
        {
            if (!tc.StartsWith("99"))
                return "Yabancı uyruklu için kimlik numarası 99 ile başlamalıdır.";
        }

        return null;
    }
}
