using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services;

// Kişi 2'nin gerçek doğrulama servisi gelene kadar kullanılan geçici servis.
public class FakeValidationService : IValidationService
{
    private readonly AppDbContext _db;

    public FakeValidationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DocumentStatus> ValidateAsync(int documentId)
    {
        // Rapor ekranını denemek için üç örnek sonuç: biri geçen, biri kalan, biri uyarı.
        _db.ValidationResults.AddRange(
            new ValidationResultRecord
            {
                DocumentId = documentId,
                RuleName = "Dosya formatı",
                FieldName = "FileType",
                IsValid = true,
                Severity = Severity.Error,
                Message = "Dosya formatı geçerli."
            },
            new ValidationResultRecord
            {
                DocumentId = documentId,
                RuleName = "TC kimlik no",
                FieldName = "TcNo",
                IsValid = false,
                Severity = Severity.Error,
                Message = "TC kimlik numarası 11 haneli olmalıdır.",
                Suggestion = "Numarayı kontrol edip tekrar girin."
            },
            new ValidationResultRecord
            {
                DocumentId = documentId,
                RuleName = "Mühendis sayısı",
                FieldName = "EngineerCount",
                IsValid = false,
                Severity = Severity.Warning,
                Message = "Mühendis sayısı 5'in altında. Mühendisin mezun olduğu bölümü belirtin."
            });

        await _db.SaveChangesAsync();

        // Sabit sonuç: hata var diye HataliEksik döner.
        // Onaylandi / Tamamlandi akışını denemek için bunu DocumentStatus.Onaylandi yapabilirsin.
        return DocumentStatus.HataliEksik;
    }
}