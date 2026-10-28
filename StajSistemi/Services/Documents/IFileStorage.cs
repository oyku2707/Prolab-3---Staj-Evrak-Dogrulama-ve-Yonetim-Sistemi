namespace StajSistemi.Services.Documents;

public interface IFileStorage
{
    Task<string> SaveAsync(IFormFile file);
}