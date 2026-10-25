using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public class Ek16Data
{
    public string? StudentFullName { get; set; }
    public string? TcNo { get; set; }
    public Nationality? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? StudentNo { get; set; }
    public string? Address { get; set; }
    public string? Department { get; set; }
    public int? PreviousInternshipDays { get; set; }
    public string? AcademicYear { get; set; }
    public string? StudentEmail { get; set; }
    public string? StudentPhone { get; set; }
    public int? HealthAidStatus{ get; set; }

    public string? CompanyName { get; set; }
    public string? ServiceArea { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyFax { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyWeb { get; set; }
    public int? EngineerCount { get; set; }
    public int? EmployeeCount { get; set; }
    public string? CompanyAddress { get; set; }
    public bool? IsAbroad { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? InternshipDays { get; set; }
    public bool? WorksOnWeekends { get; set; }
    public bool? IsPaid { get; set; }

}