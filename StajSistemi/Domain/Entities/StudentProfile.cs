using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public class StudentProfile
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string TcNo { get; set; } = "";
    public string StudentNo { get; set; } = "";
    public Nationality Nationality { get; set; }
    public DateTime BirthDate { get; set; } 
    public string Address { get; set; } = "";
    public string Department { get; set; } = "";
    public string SubmissionYear { get; set; } = "";
    public int PreviousInternshipDays { get; set; }
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public int HealthAidStatus { get; set; }
         
}
