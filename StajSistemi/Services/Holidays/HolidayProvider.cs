using StajSistemi.Data;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services.Holidays;

/// <summary>
/// Resmi tatilleri Holidays tablosundan okur.
/// </summary>
public class HolidayProvider : IHolidayProvider
{
    private readonly AppDbContext _context;

    public HolidayProvider(AppDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<DateTime> GetHolidays(DateTime start, DateTime end)
    {
        var from = start.Date;
        var to = end.Date;
        if (to < from) return [];

        return _context.Holidays
            .Where(h => h.Date >= from && h.Date <= to)
            .Select(h => h.Date)
            .ToList()
            .Select(d => d.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
    }
}