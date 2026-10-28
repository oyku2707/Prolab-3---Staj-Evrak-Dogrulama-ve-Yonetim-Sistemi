using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;

namespace StajSistemi.Services.Documents;

public class DocumentStatusService : IDocumentStatusService
{
    // Mevcut statü -> geçilebilecek statüler. Tamamlandi'dan çıkış yok.
    private static readonly Dictionary<DocumentStatus, DocumentStatus[]> Allowed = new()
    {
        { DocumentStatus.Yuklendi, new[] { DocumentStatus.Inceleniyor } },
        { DocumentStatus.Inceleniyor, new[] { DocumentStatus.HataliEksik, DocumentStatus.Onaylandi } },
        { DocumentStatus.HataliEksik, new[] { DocumentStatus.Yuklendi } },
        { DocumentStatus.Onaylandi, new[] { DocumentStatus.Tamamlandi } },
        { DocumentStatus.Tamamlandi, Array.Empty<DocumentStatus>() }
    };

    public bool CanTransition(DocumentStatus from, DocumentStatus to)
        => Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public void Transition(Document document, DocumentStatus to)
    {
        if (!CanTransition(document.Status, to))
            throw new InvalidOperationException(
                $"Geçersiz statü geçişi: {document.Status} -> {to}");

        document.Status = to;
    }

    // Tamamlandı kilidi: öğrenci için sunucu tarafında kontrol edilir.
    public bool CanStudentModify(Document document)
        => document.Status != DocumentStatus.Tamamlandi;
}