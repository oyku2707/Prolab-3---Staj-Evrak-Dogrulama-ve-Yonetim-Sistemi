using System.Globalization;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;

namespace StajSistemi.Validation.Rules;

public class ProfileMatchRule : ValidationRuleBase
{
    public const string RuleDisplayName = "Veri seti karşılaştırma";
    public override string Name => RuleDisplayName;

    private static readonly CultureInfo Tr = new("tr-TR");

    private sealed record FieldSpec(
        string Field, string Label,
        Func<Ek16Data, string?> FromDocument,
        Func<StudentProfile, string?> FromProfile,
        Func<string?, string> Normalize);

    private static string Text(string? s) =>
        string.Join(' ', (s ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpper(Tr);

    private static string Digits(string? s) =>
        new((s ?? "").Where(c => c >= '0' && c <= '9').ToArray());

    private static string Exact(string? s) => (s ?? "").Trim();

    private static readonly FieldSpec[] Fields =
    {
        new("StudentFullName", "Ad soyad", d => d.StudentFullName, p => p.FullName, Text),
        new("TcNo", "Kimlik numarası", d => d.TcNo, p => p.TcNo, Digits),
        new("Nationality", "Uyruk", d => d.Nationality?.ToString(), p => p.Nationality.ToString(), Text),
        new("StudentNo", "Öğrenci numarası", d => d.StudentNo, p => p.StudentNo, Text),
        new("BirthDate", "Doğum tarihi", d => d.BirthDate?.ToString("dd.MM.yyyy"), p => p.BirthDate.ToString("dd.MM.yyyy"), Exact),
        new("Department", "Bölüm", d => d.Department, p => p.Department, Text),
        new("StudentEmail", "E-posta", d => d.StudentEmail, p => p.Email, Text),
        new("StudentPhone", "Telefon", d => d.StudentPhone, p => p.Phone, Digits),
        new("Address", "İkametgâh adresi", d => d.Address, p => p.Address, Text),
        new("PreviousInternshipDays", "Önceki staj gün sayısı", d => d.PreviousInternshipDays?.ToString(), p => p.PreviousInternshipDays.ToString(), Exact),
        new("HealthAidStatus", "Sağlık yardımı statüsü", d => d.HealthAidStatus?.ToString(), p => p.HealthAidStatus.ToString(), Exact),
    };

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (context.Profile is null)
            return [Fail("Profile", "Öğrenci veri setinde bulunamadı, belge veri setiyle karşılaştırılamadı.", Severity.Warning)];

        var results = new List<ValidationResult>();

        foreach (var f in Fields)
        {
            var fromDoc = f.FromDocument(context.Data);
            var fromProfile = f.FromProfile(context.Profile);
            var nDoc = f.Normalize(fromDoc);
            var nProfile = f.Normalize(fromProfile);

            if (nDoc == "" && nProfile == "")
                results.Add(Pass(f.Field));
            else if (nDoc == "")
                results.Add(Fail(f.Field, $"{f.Label}: belgede bulunamadı (veri setinde: '{fromProfile}').", Severity.Warning));
            else if (nDoc != nProfile)
                results.Add(Fail(f.Field, $"{f.Label}: belgede '{fromDoc}', veri setinde '{fromProfile}' yazıyor, uyuşmuyor.", Severity.Warning));
            else
                results.Add(Pass(f.Field));
        }

        return results;
    }
}