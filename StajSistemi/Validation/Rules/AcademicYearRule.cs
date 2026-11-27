using System.Globalization;
using System.Text.RegularExpressions;

namespace StajSistemi.Validation.Rules;

public class AcademicYearRule : ValidationRuleBase
{
    // Mantıklı yıl sınırı: 0000 ya da 9999 gibi değerler tarih hesabında hata verirdi.
    private const int MinYear = 2000;
    private const int MaxYear = 2100;

    // "2025-2026 Bahar" biçimi: iki yıl, aralarında - (ya da Word'ün otomatik yaptığı uzun tire ya da /),
    // ardından yarıyıl (Güz, Bahar veya Yaz). Büyük/küçük harf ve boşluk farkı önemsizdir.
    private static readonly Regex Pattern = new(
        @"^\s*([0-9]{4})\s*[-–—/]\s*([0-9]{4})\s+(Güz|Bahar|Yaz)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public override string Name => "Öğretim yılı";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();

        results.Add(CheckFormat(context.Data.AcademicYear, out var firstYear));

        // Öğretim yılı geçerli değilse staj tarihlerini karşılaştıracağımız bir aralık da yoktur.
        if (firstYear is null)
            return results;

        // Öğretim yılı: ilk yılın 1 Eylül'ünden ertesi yılın 31 Ağustos'una kadar.
        var yearStart = new DateTime(firstYear.Value, 9, 1);
        var yearEnd = new DateTime(firstYear.Value + 1, 8, 31);

        results.AddRange(CheckDate("StartDate", "Staj başlangıç tarihi",
            context.Data.StartDate, yearStart, yearEnd));
        results.AddRange(CheckDate("EndDate", "Staj bitiş tarihi",
            context.Data.EndDate, yearStart, yearEnd));

        return results;
    }

    private ValidationResult CheckFormat(string? academicYear, out int? firstYear)
    {
        firstYear = null;

        if (string.IsNullOrWhiteSpace(academicYear))
            return Fail("AcademicYear", "Öğretim yılı boş bırakılmış (örn. 2025-2026 Bahar).");

        var match = Pattern.Match(academicYear);
        if (!match.Success)
            return Fail("AcademicYear",
                $"Öğretim yılı '{academicYear.Trim()}' beklenen biçimde değil (örn. 2025-2026 Bahar).");

        var first = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var second = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

        if (second != first + 1)
            return Fail("AcademicYear",
                $"Öğretim yılı ardışık yıllardan oluşmalıdır (örn. 2025-2026), belgede {first}-{second} yazıyor.");

        if (first < MinYear || second > MaxYear)
            return Fail("AcademicYear",
                $"Öğretim yılı ({first}-{second}) geçerli bir yıl aralığında değil.");

        firstYear = first;
        return Pass("AcademicYear");
    }

    private IEnumerable<ValidationResult> CheckDate(
        string field, string label, DateTime? date, DateTime yearStart, DateTime yearEnd)
    {
        // Tarih okunamamışsa burada tekrar hata üretmeyiz, karşılaştıracak bir değer yok.
        if (date is null)
            return [];

        var day = date.Value.Date;

        if (day < yearStart || day > yearEnd)
            return [Fail(field,
                $"{label} ({FormatDate(day)}) öğretim yılının dışında: " +
                $"{FormatDate(yearStart)} - {FormatDate(yearEnd)} arasında olmalıdır.")];

        return [Pass(field)];
    }

    private static string FormatDate(DateTime date) =>
        date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
