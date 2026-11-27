namespace StajSistemi.Domain.Interfaces;

/// <summary>
/// Seçilen hafta günleri ve tarih aralığından, resmi tatilleri çıkararak staj gün sayısını hesaplar.
/// </summary>
public interface IInternshipDayCalculator
{
    /// <param name="weekDays">
    /// InternshipDaySelection.WeekDays ile aynı biçim: "Monday,Tuesday" (virgülle ayrılmış İngilizce gün adları).
    /// </param>
    /// <param name="start">Staj başlangıç tarihi (dahil).</param>
    /// <param name="end">Staj bitiş tarihi (dahil).</param>
    /// <returns>Aralıktaki seçili günlerin sayısı, resmi tatiller düşülmüş halde.</returns>
    int Calculate(string weekDays, DateTime start, DateTime end);
}