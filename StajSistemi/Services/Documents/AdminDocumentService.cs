using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;

namespace StajSistemi.Services.Documents;

// Admin istisnası: Tamamlandı dahil silebilir. Sadece AdminController çağırır.
public class AdminDocumentService : IAdminDocumentService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminDocumentService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
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

        // Diskteki dosyayı da sil
        var fullPath = Path.Combine(_env.ContentRootPath, document.FilePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);

        return true;
    }
}