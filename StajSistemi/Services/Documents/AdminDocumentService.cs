using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;

namespace StajSistemi.Services.Documents;

// Admin istisnası: Tamamlandı dahil düzenleyebilir ve silebilir. Sadece AdminController çağırır.
public class AdminDocumentService : IAdminDocumentService
{
    private static readonly string[] AllowedExtensions =
        { ".doc", ".docx", ".pdf", ".jpg", ".jpeg", ".png" };

    private const long MaxFileSize = 10 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IFileStorage _storage;

    public AdminDocumentService(AppDbContext db, IWebHostEnvironment env, IFileStorage storage)
    {
        _db = db;
        _env = env;
        _storage = storage;
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
            return new UploadResult { Success = false, Error = "Belge bulunamadı." };

        // Dosya zorunlu değil; seçildiyse format kontrolü yapılır.
        string? newPath = null;
        string? newType = null;
        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return new UploadResult { Success = false, Error = "Bu dosya biçimi kabul edilmiyor. İzin verilenler: .doc, .docx, .pdf, .jpg, .jpeg, .png" };
            if (file.Length > MaxFileSize)
                return new UploadResult { Success = false, Error = "Dosya boyutu en fazla 10 MB olabilir." };

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

        // Statüye dokunulmaz.
        return new UploadResult { Success = true, DocumentId = document.Id };
    }

    private void DeleteFileIfExists(string path)
{
    var fullPath = Path.IsPathRooted(path)
        ? path
        : Path.Combine(_env.ContentRootPath, path);
    if (File.Exists(fullPath)) File.Delete(fullPath);
}
}