using StajSistemi.Domain.Enums;

namespace StajSistemi.Models;

public class DocumentRowViewModel
{
    public int Id { get; set; }
    public string FileType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DocumentStatus Status { get; set; }
}