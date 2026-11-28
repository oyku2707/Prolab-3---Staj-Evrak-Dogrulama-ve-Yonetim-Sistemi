namespace StajSistemi.Domain.Interfaces;

/// <summary>
/// Resmi tatil bilgisini sağlar. Gerçek uygulama Holidays tablosundan okur.
/// </summary>
public interface IHolidayProvider
{
    /// <summary>
    /// Başlangıç ve bitiş tarihi dahil, bu aralıktaki resmi tatillerin tarihlerini
    /// (saat bilgisi olmadan) artan sırada döner. Tatil yoksa boş liste döner.
    /// </summary>
    IReadOnlyList<DateTime> GetHolidays(DateTime start, DateTime end);
}