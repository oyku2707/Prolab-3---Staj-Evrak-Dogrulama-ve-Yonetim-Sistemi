using StajSistemi.Domain.Enums;

namespace StajSistemi.Helpers;

public static class StatusDisplay
{
    public static string Label(DocumentStatus status) => status switch
    {
        DocumentStatus.Yuklendi => "Yüklendi",
        DocumentStatus.Inceleniyor => "İnceleniyor",
        DocumentStatus.HataliEksik => "Hatalı / Eksik",
        DocumentStatus.Onaylandi => "Onaylandı",
        DocumentStatus.Tamamlandi => "Tamamlandı",
        _ => status.ToString()
    };

    // Bootstrap rozet sınıfı
    public static string BadgeClass(DocumentStatus status) => status switch
    {
        DocumentStatus.Yuklendi => "bg-secondary",
        DocumentStatus.Inceleniyor => "bg-primary",
        DocumentStatus.HataliEksik => "bg-danger",
        DocumentStatus.Onaylandi => "bg-success",
        DocumentStatus.Tamamlandi => "bg-success-subtle text-success-emphasis border border-success",
        _ => "bg-secondary"
    };
}