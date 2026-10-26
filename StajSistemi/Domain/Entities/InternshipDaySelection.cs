namespace StajSistemi.Domain.Entities;

public class InternshipDaySelection
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string WeekDays { get; set; } = "";
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }
    public int CalculatedDays { get; set; }
}