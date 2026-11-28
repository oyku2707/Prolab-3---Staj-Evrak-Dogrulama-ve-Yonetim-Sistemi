using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Enums;
using StajSistemi.Models;
using StajSistemi.Services;
using StajSistemi.Services.Documents;

namespace StajSistemi.Controllers;

[Authorize(Roles = "ROLE_ADMIN")]
public class AdminController : Controller
{
    private const int PageSize = 10;
    private const string DatasetRule = "Veri seti karşılaştırma";

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAdminDocumentService _adminDocs;

    public AdminController(AppDbContext db, ICurrentUserService currentUser, IAdminDocumentService adminDocs)
    {
        _db = db;
        _currentUser = currentUser;
        _adminDocs = adminDocs;
    }

    // [Authorize]'a ek ikinci kontrol: rol yoksa 403.
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_currentUser.IsAdmin)
            context.Result = new StatusCodeResult(403);
        base.OnActionExecuting(context);
    }

    // Özet sayfası: sayılar, son 7 gün, en sık başarısız kurallar, veri seti durumu.
    public async Task<IActionResult> Index()
    {
        var counts = await _db.Documents
            .GroupBy(d => d.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        var countMap = counts.ToDictionary(x => x.Status, x => x.Count);

        var byStatus = Enum.GetValues<DocumentStatus>()
            .Select(s => new StatusCount { Status = s, Count = countMap.GetValueOrDefault(s) })
            .ToList();

        var topRules = await _db.ValidationResults
            .Where(r => !r.IsValid)
            .GroupBy(r => r.RuleName)
            .Select(g => new RuleCount { RuleName = g.Key, FailedCount = g.Count() })
            .OrderByDescending(x => x.FailedCount)
            .Take(5)
            .ToListAsync();

        var since = DateTime.Today.AddDays(-6);
        var perDay = await _db.Documents
            .Where(d => d.UploadedAt >= since)
            .GroupBy(d => d.UploadedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync();
        var dayMap = perDay.ToDictionary(x => x.Day.Date, x => x.Count);

        var last7 = Enumerable.Range(0, 7)
            .Select(i => since.AddDays(i))
            .Select(day => new DayCount { Day = day, Count = dayMap.GetValueOrDefault(day.Date) })
            .ToList();

        var model = new AdminDashboardViewModel
        {
            TotalDocuments = byStatus.Sum(x => x.Count),
            ByStatus = byStatus,
            TopFailedRules = topRules,
            Last7Days = last7,
            ProfileCount = await _db.StudentProfiles.CountAsync(),
            LinkedProfileCount = await _db.StudentProfiles.CountAsync(p => p.StudentId != null)
        };
        return View(model);
    }

    public async Task<IActionResult> Documents(
        DocumentStatus? status, string? q, string? rule, DateTime? dateFrom, DateTime? dateTo, int page = 1)
    {
        var model = await BuildListAsync(status, q, rule, dateFrom, dateTo, page, false);
        return View(model);
    }

    public async Task<IActionResult> Completed(
        string? q, DateTime? dateFrom, DateTime? dateTo, int page = 1)
    {
        var model = await BuildListAsync(DocumentStatus.Tamamlandi, q, null, dateFrom, dateTo, page, true);
        return View("Documents", model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var document = await _db.Documents
            .Include(d => d.Results)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (document == null) return NotFound();

        var profile = await _db.StudentProfiles
            .FirstOrDefaultAsync(p => p.StudentId == document.StudentId);
        var hasProfile = profile != null;

        var e = document.EnteredData;
        var x = document.ExtractedData;
        var p = profile;

        var rows = new List<ComparisonRow>
        {
            Row("Ad Soyad", e?.StudentFullName, x?.StudentFullName, p?.FullName, hasProfile),
            Row("T.C. / Kimlik No", e?.TcNo, x?.TcNo, p?.TcNo, hasProfile),
            Row("Uyruk", e?.Nationality, x?.Nationality, p?.Nationality, hasProfile),
            Row("Doğum tarihi", e?.BirthDate, x?.BirthDate, p?.BirthDate, hasProfile),
            Row("Öğrenci no", e?.StudentNo, x?.StudentNo, p?.StudentNo, hasProfile),
            Row("Adres", e?.Address, x?.Address, p?.Address, hasProfile),
            Row("Bölüm", e?.Department, x?.Department, p?.Department, hasProfile),
            Row("Önceki staj günü", e?.PreviousInternshipDays, x?.PreviousInternshipDays, p?.PreviousInternshipDays, hasProfile),
            Row("E-posta", e?.StudentEmail, x?.StudentEmail, p?.Email, hasProfile),
            Row("Telefon", e?.StudentPhone, x?.StudentPhone, p?.Phone, hasProfile),
            Row("Sağlık yardımı", e?.HealthAidStatus, x?.HealthAidStatus, p?.HealthAidStatus, hasProfile),
        };

        var dataset = document.Results.Where(r => r.RuleName == DatasetRule).ToList();
        var normal = document.Results.Where(r => r.RuleName != DatasetRule).ToList();

        var model = new AdminDetailsViewModel
        {
            DocumentId = document.Id,
            FileType = document.FileType,
            UploadedAt = document.UploadedAt,
            Status = document.Status,
            HasProfile = hasProfile,
            Rows = rows,
            DatasetResults = dataset,
            Errors = normal.Where(r => !r.IsValid && r.Severity == Severity.Error).ToList(),
            Warnings = normal.Where(r => !r.IsValid && r.Severity == Severity.Warning).ToList(),
            Passed = normal.Where(r => r.IsValid).ToList()
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null) return NotFound();

        var model = new UploadViewModel
        {
            DocumentId = document.Id,
            Entered = document.EnteredData ?? new()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UploadViewModel model)
    {
        if (model.DocumentId is not int id)
            return NotFound();

        var result = await _adminDocs.UpdateAsync(id, model.Entered, model.File);
        if (!result.Success)
        {
            ViewBag.Error = result.Error;
            return View(model);
        }

        TempData["Message"] = "Belge güncellendi. Sonuçları yenilemek için Yeniden doğrula'ya basın.";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revalidate(int id)
    {
        var result = await _adminDocs.RevalidateAsync(id);
        if (result.Success)
            TempData["Message"] = "Belge yeniden doğrulandı, sonuçlar ve statü güncellendi.";
        else
            TempData["Error"] = result.Error;

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _adminDocs.DeleteAsync(id);
        TempData["Message"] = ok ? "Belge silindi." : "Belge bulunamadı.";
        return RedirectToAction("Documents");
    }

    private async Task<AdminListViewModel> BuildListAsync(
        DocumentStatus? status, string? q, string? rule,
        DateTime? dateFrom, DateTime? dateTo, int page, bool onlyCompleted)
    {
        var query = _db.Documents.AsQueryable();

        if (status != null)
            query = query.Where(d => d.Status == status);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(d =>
                d.EnteredData!.StudentNo!.Contains(term) ||
                d.EnteredData.StudentFullName!.Contains(term));
        }

        // Hata türü: seçilen kuraldan başarısız sonucu olan belgeler.
        if (!string.IsNullOrWhiteSpace(rule))
            query = query.Where(d => d.Results.Any(r => !r.IsValid && r.RuleName == rule));

        if (dateFrom != null)
        {
            var from = dateFrom.Value.Date;
            query = query.Where(d => d.UploadedAt >= from);
        }
        if (dateTo != null)
        {
            var until = dateTo.Value.Date.AddDays(1);
            query = query.Where(d => d.UploadedAt < until);
        }

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var rows = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(d => new AdminDocumentRow
            {
                Id = d.Id,
                StudentName = d.EnteredData!.StudentFullName ?? "",
                StudentNo = d.EnteredData.StudentNo ?? "",
                FileType = d.FileType,
                UploadedAt = d.UploadedAt,
                Status = d.Status
            })
            .ToListAsync();

        var ruleOptions = await _db.ValidationResults
            .Where(r => !r.IsValid)
            .Select(r => r.RuleName)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();

        return new AdminListViewModel
        {
            Rows = rows,
            Status = status,
            Q = q,
            Rule = rule,
            RuleOptions = ruleOptions,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Page = page,
            TotalPages = totalPages,
            OnlyCompleted = onlyCompleted
        };
    }

    private static string Fmt(object? value) => value switch
    {
        null => "",
        DateTime d => d.ToString("dd.MM.yyyy"),
        _ => value.ToString() ?? ""
    };

    // Dolu değerler arasında farklı olan varsa satır uyuşmazlık sayılır.
    private static ComparisonRow Row(string label, object? entered, object? extracted, object? dataset, bool hasProfile)
    {
        var e = Fmt(entered);
        var x = Fmt(extracted);
        var d = hasProfile ? Fmt(dataset) : "";

        var distinct = new[] { e, x, d }.Where(v => v != "").Distinct().Count();

        return new ComparisonRow
        {
            Label = label,
            Entered = e,
            Extracted = x,
            Dataset = hasProfile ? d : "veri setinde bulunamadı",
            Mismatch = distinct > 1
        };
    }
}