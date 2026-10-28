using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services.Documents;

public class DocumentService : IDocumentService
{
    private static readonly string[] AllowedExtensions =
        { ".doc", ".docx", ".pdf", ".jpg", ".jpeg", ".png" };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly AppDbContext _db;
    private readonly IFileStorage _storage;
    private readonly IDocumentStatusService _status;
    private readonly IValidationService _validation;

    public DocumentService(AppDbContext db, IFileStorage storage,
        IDocumentStatusService status, IValidationService validation)
    {
        _db = db;
        _storage = storage;
        _status = status;
        _validation = validation;
    }

    public async Task<UploadResult> UploadAsync(int studentId, IFormFile? file, Ek16Data entered)
    {
        var error = CheckFile(file);
        if (error != null) return Fail(error);

        var path = await _storage.SaveAsync(file!);

        var document = new Document
        {
            StudentId = studentId,
            FilePath = path,
            FileType = Path.GetExtension(file!.FileName).ToLowerInvariant(),
            UploadedAt = DateTime.Now,
            Status = DocumentStatus.Yuklendi,
            EnteredData = entered
        };
        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        await RunValidationAsync(document);
        return new UploadResult { Success = true, DocumentId = document.Id };
    }

    public async Task<UploadResult> ReuploadAsync(int documentId, int studentId, IFormFile? file, Ek16Data entered)
    {
        var document = await _db.Documents
            .Include(d => d.Results)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null || document.StudentId != studentId)
            return Fail("Belge bulunamadı.");

        // Tamamlandı kilidi: sunucu tarafında engellenir.
        if (!_status.CanStudentModify(document))
            return Fail("Bu belge tamamlandı, değiştirilemez.");

        if (document.Status != DocumentStatus.HataliEksik)
            return Fail("Sadece Hatalı / Eksik belgeler yeniden yüklenebilir.");

        var error = CheckFile(file);
        if (error != null) return Fail(error);

        _db.ValidationResults.RemoveRange(document.Results);
        document.FilePath = await _storage.SaveAsync(file!);
        document.FileType = Path.GetExtension(file!.FileName).ToLowerInvariant();
        document.UploadedAt = DateTime.Now;
        document.EnteredData = entered;

        _status.Transition(document, DocumentStatus.Yuklendi);
        await _db.SaveChangesAsync();

        await RunValidationAsync(document);
        return new UploadResult { Success = true, DocumentId = document.Id };
    }

    // Yuklendi -> Inceleniyor -> doğrulama sonucu -> (Onaylandi ise) Tamamlandi
    private async Task RunValidationAsync(Document document)
    {
        _status.Transition(document, DocumentStatus.Inceleniyor);
        await _db.SaveChangesAsync();

        var suggested = await _validation.ValidateAsync(document.Id);
        _status.Transition(document, suggested);

        if (document.Status == DocumentStatus.Onaylandi)
            _status.Transition(document, DocumentStatus.Tamamlandi);

        await _db.SaveChangesAsync();
    }

    private static string? CheckFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return "Lütfen bir dosya seçin. Boş dosya yüklenemez.";

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return "Bu dosya biçimi kabul edilmiyor. İzin verilenler: .doc, .docx, .pdf, .jpg, .jpeg, .png";

        if (file.Length > MaxFileSize)
            return "Dosya boyutu en fazla 10 MB olabilir.";

        return null;
    }

    private static UploadResult Fail(string error) => new() { Success = false, Error = error };
}