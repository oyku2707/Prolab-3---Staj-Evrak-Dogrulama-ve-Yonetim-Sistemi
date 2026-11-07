using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Models;
using StajSistemi.Services;
using StajSistemi.Services.Documents;
using Microsoft.AspNetCore.Authorization;

namespace StajSistemi.Controllers;

[Authorize(Roles = "ROLE_STUDENT,ROLE_ADMIN")]
public class DocumentsController : Controller
{
    private static readonly string[] ValidDays =
        { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    private readonly IDocumentService _documents;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext _db;

    public DocumentsController(IDocumentService documents, ICurrentUserService currentUser, AppDbContext db)
    {
        _documents = documents;
        _currentUser = currentUser;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int? id)
    {
        var model = new UploadViewModel();

        if (id != null)
        {
            var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null || document.StudentId != _currentUser.UserId)
                return NotFound();
            if (document.Status != DocumentStatus.HataliEksik)
                return RedirectToAction("Report", new { id });

            model.DocumentId = document.Id;
            model.Entered = document.EnteredData ?? new();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadViewModel model)
    {
        if (_currentUser.UserId is not int studentId)
            return Unauthorized();

        var result = model.DocumentId is int docId
            ? await _documents.ReuploadAsync(docId, studentId, model.File, model.Entered)
            : await _documents.UploadAsync(studentId, model.File, model.Entered);

        if (!result.Success)
        {
            ViewBag.Error = result.Error;
            return View(model);
        }

        return RedirectToAction("Report", new { id = result.DocumentId });
    }

    [HttpGet]
    public async Task<IActionResult> Report(int id)
    {
        var document = await _db.Documents
            .Include(d => d.Results)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return NotFound();

        // Öğrenci sadece kendi belgesini görebilir, admin hepsini.
        if (!_currentUser.IsAdmin && document.StudentId != _currentUser.UserId)
            return NotFound();

        // Veri seti karşılaştırma sonuçları öğrenciden gizlenir, adminde gösterilir.
        var results = document.Results
            .Where(r => _currentUser.IsAdmin || r.RuleName != "Veri seti karşılaştırma")
            .ToList();

        var model = new ReportViewModel
        {
            DocumentId = document.Id,
            FileType = document.FileType,
            UploadedAt = document.UploadedAt,
            Status = document.Status,
            Errors = results.Where(r => !r.IsValid && r.Severity == Severity.Error).ToList(),
            Warnings = results.Where(r => !r.IsValid && r.Severity == Severity.Warning).ToList(),
            Passed = results.Where(r => r.IsValid).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleForm(int id)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null || document.StudentId != _currentUser.UserId)
            return NotFound();

        return View(new GoogleFormViewModel { DocumentId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoogleForm(GoogleFormViewModel model)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == model.DocumentId);
        if (document == null || document.StudentId != _currentUser.UserId)
            return NotFound();

        var url = model.Url?.Trim();
        var ok = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                 && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                 && (uri.Host == "docs.google.com" || uri.Host == "forms.gle");

        model.Valid = ok;
        model.Error = ok ? null : "Geçersiz adres. Lütfen bir Google Form bağlantısı girin (docs.google.com veya forms.gle).";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> InternshipDays(int id)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null || document.StudentId != _currentUser.UserId)
            return NotFound();

        var model = new InternshipDaysViewModel { DocumentId = id };

        var existing = await _db.InternshipDaySelections.FirstOrDefaultAsync(s => s.DocumentId == id);
        if (existing != null)
        {
            model.SelectedDays = existing.WeekDays
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            model.RangeStart = existing.RangeStart;
            model.RangeEnd = existing.RangeEnd;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InternshipDays(InternshipDaysViewModel model)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == model.DocumentId);
        if (document == null || document.StudentId != _currentUser.UserId)
            return NotFound();

        // Tamamlandı kilidi: öğrenci değiştiremez.
        if (document.Status == DocumentStatus.Tamamlandi)
        {
            model.Error = "Bu belge tamamlandı, değiştirilemez.";
            return View(model);
        }

        // Sadece geçerli gün adları kabul edilir.
        var days = model.SelectedDays.Where(d => ValidDays.Contains(d)).Distinct().ToList();

        if (days.Count == 0)
        {
            model.Error = "En az bir gün seçmelisiniz.";
            return View(model);
        }
        if (model.RangeStart == null || model.RangeEnd == null)
        {
            model.Error = "Başlangıç ve bitiş tarihini girin.";
            return View(model);
        }
        if (model.RangeEnd < model.RangeStart)
        {
            model.Error = "Bitiş tarihi başlangıç tarihinden önce olamaz.";
            return View(model);
        }

        // Haftalık günleri sözleşmedeki biçimle sakla: Monday,Tuesday
        var weekDays = string.Join(",", ValidDays.Where(days.Contains));

        var selection = await _db.InternshipDaySelections.FirstOrDefaultAsync(s => s.DocumentId == model.DocumentId);
        if (selection == null)
        {
            selection = new InternshipDaySelection { DocumentId = model.DocumentId };
            _db.InternshipDaySelections.Add(selection);
        }

        selection.WeekDays = weekDays;
        selection.RangeStart = model.RangeStart.Value;
        selection.RangeEnd = model.RangeEnd.Value;
        // CalculatedDays Kişi 2'nin hesaplayıcısı gelince doldurulacak.

        await _db.SaveChangesAsync();

        model.SelectedDays = days;
        model.Saved = true;
        return View(model);
    }
}