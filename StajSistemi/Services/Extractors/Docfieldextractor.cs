using System.Diagnostics;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services.Extractors;

// Eski Word biçimini (.doc) okur. OpenXml yalnızca .docx okuyabildiği için dosya önce
// LibreOffice ile geçici bir .docx'e çevrilir, sonra mevcut DocxFieldExtractor kullanılır.
// LibreOffice kurulu değilse ya da çeviri başarısız olursa InvalidOperationException fırlatır;
// ValidationService bunu yakalayıp raporda "belge okunamadı" sonucuna çevirir.
public class DocFieldExtractor : IDocumentFieldExtractor
{
    private const int TimeoutMs = 60_000;

    private readonly string? _configuredPath;
    private readonly DocxFieldExtractor _docx = new();

    public DocFieldExtractor(IConfiguration configuration)
    {
        // İsteğe bağlı: dotnet user-secrets set "LibreOffice:Path" "C:\...\soffice.exe"
        _configuredPath = configuration["LibreOffice:Path"];
    }

    public bool Supports(string fileExtension) =>
        fileExtension.Equals(".doc", StringComparison.OrdinalIgnoreCase);

    public Ek16Data Extract(string filePath)
    {
        var soffice = FindSoffice()
            ?? throw new InvalidOperationException(
                ".doc okumak için LibreOffice gerekli ama bulunamadı.");

        var workDir = Path.Combine(Path.GetTempPath(), "doc2docx_" + Guid.NewGuid().ToString("N"));
        var profileDir = Path.Combine(workDir, "profile");
        var outDir = Path.Combine(workDir, "out");
        Directory.CreateDirectory(profileDir);
        Directory.CreateDirectory(outDir);

        try
        {
            var psi = new ProcessStartInfo(soffice)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            // Ayrı profil: açık bir LibreOffice penceresiyle çakışmasın.
            psi.ArgumentList.Add("-env:UserInstallation=" + new Uri(profileDir).AbsoluteUri);
            psi.ArgumentList.Add("--headless");
            psi.ArgumentList.Add("--convert-to");
            psi.ArgumentList.Add("docx");
            psi.ArgumentList.Add("--outdir");
            psi.ArgumentList.Add(outDir);
            psi.ArgumentList.Add(filePath);

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("LibreOffice başlatılamadı.");

            if (!process.WaitForExit(TimeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                throw new InvalidOperationException(".doc çevirisi zaman aşımına uğradı.");
            }

            var converted = Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".docx");
            if (!File.Exists(converted))
                throw new InvalidOperationException(".doc dosyası .docx'e çevrilemedi.");

            return _docx.Extract(converted);
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); } catch { }
        }
    }

    private string? FindSoffice()
    {
        if (!string.IsNullOrWhiteSpace(_configuredPath) && File.Exists(_configuredPath))
            return _configuredPath;

        var candidates = new List<string>();

        // Windows
        foreach (var root in new[]
        {
            Environment.GetEnvironmentVariable("ProgramFiles"),
            Environment.GetEnvironmentVariable("ProgramFiles(x86)")
        })
        {
            if (!string.IsNullOrEmpty(root))
                candidates.Add(Path.Combine(root, "LibreOffice", "program", "soffice.exe"));
        }

        // macOS ve Linux
        candidates.Add("/Applications/LibreOffice.app/Contents/MacOS/soffice");
        candidates.Add("/usr/bin/soffice");
        candidates.Add("/usr/local/bin/soffice");

        foreach (var c in candidates)
            if (File.Exists(c)) return c;

        // PATH içinde ara
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var name in new[] { "soffice", "soffice.exe" })
            {
                var full = Path.Combine(dir.Trim(), name);
                if (File.Exists(full)) return full;
            }
        }

        return null;
    }
}
