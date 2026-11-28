using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services.Documents;

// Admin istisnası: Tamamlandı dahil düzenleyebilir, silebilir, yeniden doğrulayabilir.
// Sadece AdminController çağırır.
public class AdminDocumentService : IAdminDocumentService
{
    private static readonly string[] AllowedExtensions =
        { ".doc", ".docx", ".pdf", ".jpg", ".jpeg", ".png" };

    private const long MaxFileSize = 10 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IFileStorage _storage;
    private readonly IValidationService _validation;
    private readonly IDocumentStatusService _status;

    public AdminDocumentService(AppDbContext db, IWebHostEnvironment env, IFileStorage storage,
        IValidationService validation, IDocumentStatusService status)
    {
        _db = db;
        _env = env;
        _storage = storage;
        _validation = validation;
        _status = status;
    }

    public async Task<bool> DeleteAsync(int documentId)
    {
        var document = await _db.Documents
            .Include(d => d.Results)
            .FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return false;

        _db.ValidationResults.RemoveRange(document.Results);
        _db.InternshipDaySelections.RemoveRange(
            _db.InternshipDaySelections.Where(s => s.DocumentId == documentId));
        _db.Documents.Remove(document);
        await _db.SaveChangesAsync();

        DeleteFileIfExists(document.FilePath);
        return true;
    }

    public async Task<UploadResult> UpdateAsync(int documentId, Ek16Data entered, IFormFile? file)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null)
            return Fail("Belge bulunamadı.");

        var entryError = EntryChecks.Validate(entered);
        if (entryError != null)
            return Fail(entryError);

        // Dosya zorunlu değil; seçildiyse format kontrolü yapılır.
        string? newPath = null;
        string? newType = null;
        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return Fail("Bu dosya biçimi kabul edilmiyor. İzin verilenler: .doc, .docx, .pdf, .jpg, .jpeg, .png");
            if (file.Length > MaxFileSize)
                return Fail("Dosya boyutu en fazla 10 MB olabilir.");

            newPath = await _storage.SaveAsync(file);
            newType = ext;
        }

        document.EnteredData = entered;

        if (newPath != null)
        {
            var oldPath = document.FilePath;
            document.FilePath = newPath;
            document.FileType = newType!;
            document.UploadedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            DeleteFileIfExists(oldPath);
        }
        else
        {
            await _db.SaveChangesAsync();
        }

        // Düzenleme statüye dokunmaz. Sonuçları güncellemek için RevalidateAsync kullanılır.
        return new UploadResult { Success = true, DocumentId = document.Id };
    }

    public async Task<UploadResult> RevalidateAsync(int documentId)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null)
            return Fail("Belge bulunamadı.");

        DocumentStatus suggested;
        try
        {
            // ValidateAsync eski sonuçları kendisi siler ve yenilerini yazar.
            suggested = await _validation.ValidateAsync(documentId);
        }
        catch (Exception)
        {
            return Fail("Doğrulama çalışırken hata oluştu, statü değiştirilmedi.");
        }

        // Hata yoksa belge Tamamlandı, varsa Hatalı/Eksik olur.
        var target = suggested == DocumentStatus.Onaylandi
            ? DocumentStatus.Tamamlandi
            : DocumentStatus.HataliEksik;

        _status.AdminSet(document, target);
        await _db.SaveChangesAsync();

        return new UploadResult { Success = true, DocumentId = document.Id };
    }

    private static UploadResult Fail(string error) => new() { Success = false, Error = error };

    private void DeleteFileIfExists(string path)
    {
        var fullPath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(_env.ContentRootPath, path);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}