using System.Globalization;
using StajSistemi.Domain.Entities;

namespace StajSistemi.Validation.Rules;

public class FormVsDocumentRule : ValidationRuleBase
{
    public const string RuleDisplayName = "Form ve belge karşılaştırma";
    public override string Name => RuleDisplayName;

    private static readonly CultureInfo Tr = new("tr-TR");

    private sealed record FieldSpec(
        string Field, string Label,
        Func<Ek16Data, string?> Get,
        Func<string?, string> Normalize);

    private static string Text(string? s) =>
        string.Join(' ', (s ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpper(Tr);

    private static string Digits(string? s) =>
        new((s ?? "").Where(c => c >= '0' && c <= '9').ToArray());

    private static string Exact(string? s) => (s ?? "").Trim();

    private static readonly FieldSpec[] Fields =
    {
        new("StudentFullName", "Ad soyad", d => d.StudentFullName, Text),
        new("TcNo", "Kimlik numarası", d => d.TcNo, Digits),
        new("Nationality", "Uyruk", d => d.Nationality?.ToString(), Text),
        new("BirthDate", "Doğum tarihi", d => d.BirthDate?.ToString("dd.MM.yyyy"), Exact),
        new("StudentNo", "Öğrenci numarası", d => d.StudentNo, Text),
        new("Department", "Bölüm", d => d.Department, Text),
        new("StudentEmail", "Öğrenci e-postası", d => d.StudentEmail, Text),
        new("StudentPhone", "Öğrenci telefonu", d => d.StudentPhone, Digits),
        new("CompanyName", "İşyeri adı", d => d.CompanyName, Text),
        new("CompanyPhone", "İşyeri telefonu", d => d.CompanyPhone, Digits),
        new("CompanyEmail", "İşyeri e-postası", d => d.CompanyEmail, Text),
        new("EngineerCount", "Mühendis sayısı", d => d.EngineerCount?.ToString(), Exact),
        new("StartDate", "Staj başlama tarihi", d => d.StartDate?.ToString("dd.MM.yyyy"), Exact),
        new("EndDate", "Staj bitiş tarihi", d => d.EndDate?.ToString("dd.MM.yyyy"), Exact),
        new("InternshipDays", "Staj gün sayısı", d => d.InternshipDays?.ToString(), Exact),
        new("EmployerFullName", "İşveren adı soyadı", d => d.EmployerFullName, Text),
        new("EmployerEmail", "İşveren e-postası", d => d.EmployerEmail, Text),
        new("EmployerPhone", "İşveren telefonu", d => d.EmployerPhone, Digits),
    };

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (context.Entered is null)
            return [Fail("Form", "Form bilgileri girilmemiş, belge ile karşılaştırılamadı.")];

        var results = new List<ValidationResult>();

        foreach (var f in Fields)
        {
            var form = f.Get(context.Entered);
            var doc = f.Get(context.Data);
            var nForm = f.Normalize(form);
            var nDoc = f.Normalize(doc);

            if (nForm == "" && nDoc == "")
                results.Add(Pass(f.Field));
            else if (nDoc == "")
                results.Add(Fail(f.Field, $"{f.Label}: belgede bulunamadı (formda: '{form}')."));
            else if (nForm == "")
                results.Add(Fail(f.Field, $"{f.Label}: formda boş bırakılmış (belgede: '{doc}')."));
            else if (nForm != nDoc)
                results.Add(Fail(f.Field, $"{f.Label}: belgede '{doc}', formda '{form}' yazıyor, uyuşmuyor."));
            else
                results.Add(Pass(f.Field));
        }

        return results;
    }
}