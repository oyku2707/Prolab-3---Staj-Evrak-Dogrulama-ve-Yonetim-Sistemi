using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services.Extractors;

// JPEG dosyaları için okuyucu iskeleti.
// Gerçek OCR ile alan okuma final aşamasında eklenecek.
public class JpegFieldExtractor : IDocumentFieldExtractor
{
    private readonly ILogger<JpegFieldExtractor> _logger;

    public JpegFieldExtractor(ILogger<JpegFieldExtractor> logger)
    {
        _logger = logger;
    }

    public bool Supports(string fileExtension) =>
        fileExtension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        fileExtension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);

    public Ek16Data Extract(string filePath)
    {
        // TODO: OCR (Tesseract) ile metin okuma ve alanları doldurma.
        _logger.LogInformation("JPEG OCR henüz uygulanmadı: {File}", filePath);
        return new Ek16Data();
    }
}