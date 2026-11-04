using StajSistemi.Domain.Enums;

namespace StajSistemi.Validation.Rules;

public class TcKimlikRule : ValidationRuleBase
{
    public override string Name => "Kimlik numarası";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        return [CheckTc(context)];
    }

    private ValidationResult CheckTc(ValidationContext context)
    {
        var tc = context.Data.TcNo?.Trim();
        var nationality = context.Data.Nationality;

        if (string.IsNullOrWhiteSpace(tc))
            return Fail("TcNo", "Kimlik numarası boş bırakılmış.");

        if (nationality is null)
            return Fail("Nationality", "Uyruk belirtilmemiş, kimlik numarası kontrol edilemedi.");

        if (tc.Length != 11 || !tc.All(c => c >= '0' && c <= '9'))
            return Fail("TcNo", "Kimlik numarası 11 haneli olmalı ve sadece rakamlardan oluşmalıdır.");

        if (nationality == Nationality.TC)
        {
            if (tc[0] == '0')
                return Fail("TcNo", "T.C. uyruklu için kimlik numarasının ilk hanesi 0 olamaz.");

            var digits = tc.Select(c => c - '0').ToArray();
            var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
            var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
            var tenth = ((oddSum * 7 - evenSum) % 10 + 10) % 10;
            var eleventh = digits.Take(10).Sum() % 10;

            if (digits[9] != tenth || digits[10] != eleventh)
                return Fail("TcNo", "Kimlik numarası geçerli bir T.C. kimlik numarası değil.");
        }
        else
        {
            if (!tc.StartsWith("99"))
                return Fail("TcNo", "Yabancı uyruklu için kimlik numarası 99 ile başlamalıdır.");
        }

        return Pass("TcNo");
    }
}