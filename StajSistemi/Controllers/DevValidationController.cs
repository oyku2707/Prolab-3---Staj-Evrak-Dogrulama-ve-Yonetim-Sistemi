using System.Text;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Services;
using StajSistemi.Validation;

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
            TcNo = "10000000147", Nationality = Nationality.TC,
            StudentPhone = "0532 123 45 67", CompanyPhone = "+905321234567", EmployerPhone = "+90 532 123 45",
            StudentEmail = "ad@alanadi", CompanyEmail = "ad@@alanadi.com", EmployerEmail = "ad alanadi@x.com",
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

    private static Ek16Data Baseline() => new()
    {
        TcNo = "10000000146", Nationality = Nationality.TC,
        StudentPhone = "+90 532 123 45 67", CompanyPhone = "+90 212 123 45 67", EmployerPhone = "+90 216 123 45 67",
        StudentEmail = "ogrenci@univ.edu.tr", CompanyEmail = "info@firma.com", EmployerEmail = "yetkili@firma.com",
        EngineerCount = 6
    };
}