using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string FilePath { get; set; } = "";
    public string FileType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Yuklendi;

    public Ek16Data? ExtractedData { get; set; }
    public Ek16Data? EnteredData { get; set; }
    public List<ValidationResultRecord> Results { get; set; } = new();
}