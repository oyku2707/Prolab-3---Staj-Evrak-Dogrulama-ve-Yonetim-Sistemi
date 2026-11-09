namespace StajSistemi.Services.Documents;

public class FileStorage : IFileStorage
{
    private readonly string _uploadsFolder;

    public FileStorage(IWebHostEnvironment env)
    {
        // wwwroot dışında: dosyalar doğrudan adresle indirilemez.
        _uploadsFolder = Path.Combine(env.ContentRootPath, "Uploads");
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        Directory.CreateDirectory(_uploadsFolder);

        // Kullanıcının verdiği ad yola konmaz, özgün ad üretilir.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = Guid.NewGuid() + extension;
        var fullPath = Path.Combine(_uploadsFolder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Tam yol döner: doğrulama servisi Document.FilePath ile dosyayı doğrudan açabilir.
        return fullPath;
    }
}