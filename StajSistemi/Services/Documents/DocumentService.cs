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
        // 1) Format kontrolleri
        if (file == null || file.Length == 0)
            return Fail("Lütfen bir dosya seçin. Boş dosya yüklenemez.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return Fail("Bu dosya biçimi kabul edilmiyor. İzin verilenler: .doc, .docx, .pdf, .jpg, .jpeg, .png");

        if (file.Length > MaxFileSize)
            return Fail("Dosya boyutu en fazla 10 MB olabilir.");

        // 2) Kaydet, Document oluştur (EnteredData doğrulamadan ÖNCE kaydedilir)
        var path = await _storage.SaveAsync(file);

        var document = new Document
        {
            StudentId = studentId,
            FilePath = path,
            FileType = extension,
            UploadedAt = DateTime.Now,
            Status = DocumentStatus.Yuklendi,
            EnteredData = entered
        };
        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        // 3) Yuklendi -> Inceleniyor
        _status.Transition(document, DocumentStatus.Inceleniyor);
        await _db.SaveChangesAsync();

        // 4) Doğrulama, dönen önerilen statüyü uygula
        var suggested = await _validation.ValidateAsync(document.Id);
        _status.Transition(document, suggested);

        // 5) Onaylandı ise hemen Tamamlandı
        if (document.Status == DocumentStatus.Onaylandi)
            _status.Transition(document, DocumentStatus.Tamamlandi);

        await _db.SaveChangesAsync();

        return new UploadResult { Success = true, DocumentId = document.Id };
    }

    private static UploadResult Fail(string error) => new() { Success = false, Error = error };
}