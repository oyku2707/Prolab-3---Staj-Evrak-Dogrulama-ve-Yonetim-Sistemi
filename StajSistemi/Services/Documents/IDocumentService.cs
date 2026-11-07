using StajSistemi.Domain.Entities;

namespace StajSistemi.Services.Documents;

public class UploadResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public int DocumentId { get; init; }
}

public interface IDocumentService
{
    Task<UploadResult> UploadAsync(int studentId, IFormFile? file, Ek16Data entered);
    Task<UploadResult> ReuploadAsync(int documentId, int studentId, IFormFile? file, Ek16Data entered);
}