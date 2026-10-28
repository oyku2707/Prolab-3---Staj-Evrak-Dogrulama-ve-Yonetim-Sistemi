namespace StajSistemi.Services.Documents;

public interface IAdminDocumentService
{
    Task<bool> DeleteAsync(int documentId);
}