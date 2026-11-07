using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;

namespace StajSistemi.Models;

public class ReportViewModel
{
    public int DocumentId { get; set; }
    public string FileType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DocumentStatus Status { get; set; }

    public List<ValidationResultRecord> Errors { get; set; } = new();
    public List<ValidationResultRecord> Warnings { get; set; } = new();
    public List<ValidationResultRecord> Passed { get; set; } = new();
}