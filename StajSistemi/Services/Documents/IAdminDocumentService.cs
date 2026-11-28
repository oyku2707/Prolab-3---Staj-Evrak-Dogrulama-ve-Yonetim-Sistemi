using StajSistemi.Domain.Entities;

namespace StajSistemi.Services.Documents;

public interface IAdminDocumentService
{
    Task<bool> DeleteAsync(int documentId);
    Task<UploadResult> UpdateAsync(int documentId, Ek16Data entered, IFormFile? file);
    Task<UploadResult> RevalidateAsync(int documentId);
}