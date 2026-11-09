namespace StajSistemi.Models;

public class InternshipDaysViewModel
{
    public int DocumentId { get; set; }

    // Seçilen günler: "Monday", "Tuesday" ... (DayOfWeek adları)
    public List<string> SelectedDays { get; set; } = new();

    public DateTime? RangeStart { get; set; }
    public DateTime? RangeEnd { get; set; }

    public string? Error { get; set; }
    public bool Saved { get; set; }
}