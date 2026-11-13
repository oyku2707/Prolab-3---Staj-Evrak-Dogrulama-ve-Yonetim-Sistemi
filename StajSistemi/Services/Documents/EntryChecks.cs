using StajSistemi.Domain.Entities;

namespace StajSistemi.Services.Documents;

// Formdan gelen bilgilerin sunucu tarafı kontrolleri.
public static class EntryChecks
{
    public static string? Validate(Ek16Data data)
    {
        if (data.PreviousInternshipDays < 0)
            return "Önceki staj günü sayısı negatif olamaz.";
        if (data.EngineerCount < 0)
            return "Mühendis sayısı negatif olamaz.";
        if (data.EmployeeCount < 0)
            return "Çalışan sayısı negatif olamaz.";
        if (data.InternshipDays < 0)
            return "Staj gün sayısı negatif olamaz.";
        return null;
    }
}