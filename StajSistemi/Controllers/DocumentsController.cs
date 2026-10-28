using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Enums;
using StajSistemi.Models;
using StajSistemi.Services;
using StajSistemi.Services.Documents;

namespace StajSistemi.Controllers;

// TODO: Kişi 1'in girişi gelince [Authorize] eklenecek.
public class DocumentsController : Controller
{
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
}