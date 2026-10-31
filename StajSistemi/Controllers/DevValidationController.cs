using System.Text;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Services;
using StajSistemi.Validation;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StajSistemi.Services.Extractors;
using StajSistemi.Validation.Rules;

namespace StajSistemi.Controllers;

// GEÇİCİ: sadece deneme için. Main'e GÖNDERİLMEYECEK, Pull Request'ten önce silinecek.
public class DevValidationController : Controller
{
    private readonly ValidationService _service;
    private readonly IWebHostEnvironment _env;

    public DevValidationController(ValidationService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    public IActionResult Run()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var cases = new List<(string Name, Ek16Data Data)>();

        cases.Add(("1) Hepsi doğru", Baseline()));

        var c2 = new Ek16Data
        {
            TcNo = "10000000147",
            Nationality = Nationality.TC,
            StudentPhone = "0532 123 45 67",
            CompanyPhone = "+905321234567",
            EmployerPhone = "+90 532 123 45",
            StudentEmail = "ad@alanadi",
            CompanyEmail = "ad@@alanadi.com",
            EmployerEmail = "ad alanadi@x.com",
            EngineerCount = 3
        };
        cases.Add(("2) Hepsi hatalı", c2));

        cases.Add(("3) Hiçbir alan doldurulmamış", new Ek16Data()));

        var c4 = Baseline();
        c4.Nationality = Nationality.Yabanci;
        c4.TcNo = "99000000001";
        c4.EngineerCount = 5;
        cases.Add(("4) Yabancı uyruklu, geçerli", c4));

        var c5 = Baseline();
        c5.Nationality = Nationality.Yabanci;
        c5.TcNo = "12000000001";
        cases.Add(("5) Yabancı uyruklu, 99 ile başlamıyor", c5));

        var c6 = Baseline();
        c6.Nationality = null;
        cases.Add(("6) Uyruk belirtilmemiş", c6));

        var sb = new StringBuilder();
        foreach (var (name, data) in cases)
        {
            sb.AppendLine($"=== {name} ===");
            var context = new ValidationContext { Data = data };

            foreach (var r in _service.RunRules(context))
            {
                var tag = r.IsValid ? "GEÇTİ" : r.Severity == Severity.Warning ? "UYARI" : "HATA ";
                sb.AppendLine($"[{tag}] {r.RuleName} / {r.FieldName}: {r.Message}");
            }
            sb.AppendLine();
        }

        return Content(sb.ToString(), "text/plain; charset=utf-8");
    }
    public IActionResult DumpDocx()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var path = Path.Combine(_env.ContentRootPath, "TestBelgeleri", "ornek1.docx");
        if (!System.IO.File.Exists(path))
            return Content("Dosya bulunamadı: " + path);

        var sb = new StringBuilder();
        using var wp = WordprocessingDocument.Open(path, false);
        var body = wp.MainDocumentPart!.Document.Body!;
        var tables = body.Elements<Table>().ToList();

        for (int t = 0; t < tables.Count; t++)
        {
            sb.AppendLine($"=== TABLO {t} ===");
            var rows = tables[t].Elements<TableRow>().ToList();
            for (int r = 0; r < rows.Count; r++)
            {
                var cells = rows[r].Elements<TableCell>().Select(c => c.InnerText.Trim()).ToList();
                sb.AppendLine($"[{r}] " + string.Join(" | ", cells));
            }
        }
        sb.AppendLine();
        sb.AppendLine("=== TABLO DIŞI PARAGRAFLAR ===");
        foreach (var p in body.Elements<Paragraph>())
        {
            var text = p.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
                sb.AppendLine("- " + text);
        }

        return Content(sb.ToString(), "text/plain; charset=utf-8");
    }
    public IActionResult ExtractDocx()
{
    if (!_env.IsDevelopment())
        return NotFound();

    var path = Path.Combine(_env.ContentRootPath, "TestBelgeleri", "ornek2_hatali.docx");
    if (!System.IO.File.Exists(path))
        return Content("Dosya bulunamadı: " + path);

    var sb = new StringBuilder();

    using (var wp = WordprocessingDocument.Open(path, false))
    {
        var tables = wp.MainDocumentPart!.Document.Body!.Elements<Table>().ToList();
        sb.AppendLine("Tablo sayısı: " + tables.Count);
        var body = wp.MainDocumentPart!.Document.Body!;
        sb.AppendLine("Descendants<Table> sayısı: " + body.Descendants<Table>().Count());
        foreach (var g in body.ChildElements.GroupBy(e => e.GetType().Name))
        sb.AppendLine($"  Body çocuğu: {g.Key} x{g.Count()}");

        if (tables.Count > 0)
        {
            var rows = tables[0].Elements<TableRow>().ToList();
            sb.AppendLine("Tablo 0 satır sayısı: " + rows.Count);

            if (rows.Count > 1)
            {
                var cells = rows[1].Elements<TableCell>().ToList();
                sb.AppendLine("2. satırdaki hücre sayısı: " + cells.Count);
                foreach (var c in cells)
                {
                    var text = c.InnerText;
                    var codes = string.Join(" ", text.Take(12).Select(ch => ((int)ch).ToString("X4")));
                    sb.AppendLine($"  hücre: '{text}' | ilk 12 karakterin kodu: {codes}");
                }
            }
        }
    }

    sb.AppendLine();
    sb.AppendLine("--- Extractor sonucu ---");
    var extractor = new DocxFieldExtractor();
    var data = extractor.Extract(path);
    sb.AppendLine("Extractor sürümü: " + DocxFieldExtractor.Version);
    foreach (var line in extractor.Log) sb.AppendLine("  " + line);
    foreach (var p in typeof(Ek16Data).GetProperties())
        sb.AppendLine($"{p.Name} = {p.GetValue(data) ?? "(boş)"}");

    return Content(sb.ToString(), "text/plain; charset=utf-8");
}
public IActionResult CompareDocx()
{
    if (!_env.IsDevelopment())
        return NotFound();

    var path = Path.Combine(_env.ContentRootPath, "TestBelgeleri", "ornek1.docx");
    if (!System.IO.File.Exists(path))
        return Content("Dosya bulunamadı: " + path);

    var extractor = new DocxFieldExtractor();
    var data = extractor.Extract(path);       // belgeden okunan
    var entered = extractor.Extract(path);    // "formdan girilmiş" gibi: aynı bilgiler...

    // ...ve beş bilinçli fark:
    entered.StudentFullName = "Deneme Öğrencii";    // yazım farkı
    entered.StudentPhone = "+90 532 999 99 99";     // farklı numara
    entered.EngineerCount = 7;                      // farklı sayı
    entered.CompanyEmail = null;                    // formda boş
    data.EmployerPhone = null;                      // belgede okunamamış gibi

    var context = new ValidationContext { Data = data, Entered = entered };

    var sb = new StringBuilder();
    foreach (var r in _service.RunRules(context)
                 .Where(r => r.RuleName == FormVsDocumentRule.RuleDisplayName))
    {
        var tag = r.IsValid ? "GEÇTİ" : "HATA ";
        sb.AppendLine($"[{tag}] {r.FieldName}: {r.Message}");
    }

    return Content(sb.ToString(), "text/plain; charset=utf-8");
}

    private static Ek16Data Baseline() => new()
    {
        TcNo = "10000000146",
        Nationality = Nationality.TC,
        StudentPhone = "+90 532 123 45 67",
        CompanyPhone = "+90 212 123 45 67",
        EmployerPhone = "+90 216 123 45 67",
        StudentEmail = "ogrenci@univ.edu.tr",
        CompanyEmail = "info@firma.com",
        EmployerEmail = "yetkili@firma.com",
        EngineerCount = 6
    };
}