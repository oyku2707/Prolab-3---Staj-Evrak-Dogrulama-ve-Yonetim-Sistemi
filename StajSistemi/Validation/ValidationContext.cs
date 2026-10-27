using StajSistemi.Domain.Entities;

namespace StajSistemi.Validation;

public class ValidationContext
{
    public Ek16Data Data { get; init; } = new();
    public StudentProfile? Profile { get; init; }
    public List<DayOfWeek> SelectedWeekDays { get; init; } = new();
    public DateTime SubmissionDate { get; init; }
}