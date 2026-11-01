using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;
using StajSistemi.Validation;

namespace StajSistemi.Services;

public class ValidationService : IValidationService
{
    private readonly IEnumerable<IValidationRule> _rules;
    private readonly IEnumerable<IDocumentFieldExtractor> _extractors;
    private readonly AppDbContext _db;

    public ValidationService(
        IEnumerable<IValidationRule> rules,
        IEnumerable<IDocumentFieldExtractor> extractors,
        AppDbContext db)
    {
        _rules = rules;
        _extractors = extractors;
        _db = db;
    }

    public List<ValidationResult> RunRules(ValidationContext context)
    {
        var results = new List<ValidationResult>();

        foreach (var rule in _rules)
            results.AddRange(rule.Validate(context));

        return results;
    }

    public async Task<DocumentStatus> ValidateAsync(int documentId)
    {
        var document = await _db.Documents
            .Include(d => d.Results)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            throw new ArgumentException($"Belge bulunamadı (Id: {documentId}).");

        var results = new List<ValidationResult>();

        var extension = Path.GetExtension(document.FilePath);
        var extractor = _extractors.FirstOrDefault(e => e.Supports(extension));

        if (extractor is null)
        {
            results.Add(ReadFailure("Bu dosya biçimi vizede otomatik okunamıyor (şimdilik sadece .docx desteklenir)."));
        }
        else
        {
            try
            {
                document.ExtractedData = extractor.Extract(document.FilePath);
            }
            catch (Exception)
            {
                results.Add(ReadFailure("Belge okunamadı, dosya bozuk ya da beklenen biçimde olmayabilir."));
            }
        }

        if (results.Count == 0)
        {
            var profile = await _db.StudentProfiles
                .FirstOrDefaultAsync(p => p.StudentId == document.StudentId);

            var context = new ValidationContext
            {
                Data = document.ExtractedData ?? new Ek16Data(),
                Entered = document.EnteredData,
                Profile = profile,
                SubmissionDate = document.UploadedAt
            };

            results.AddRange(RunRules(context));
        }

        _db.ValidationResults.RemoveRange(document.Results);

        foreach (var r in results)
        {
            _db.ValidationResults.Add(new ValidationResultRecord
            {
                DocumentId = document.Id,
                RuleName = r.RuleName,
                FieldName = r.FieldName,
                IsValid = r.IsValid,
                Severity = r.Severity,
                Message = r.Message,
                Suggestion = r.Suggestion
            });
        }

        await _db.SaveChangesAsync();

        var hasError = results.Any(r => !r.IsValid && r.Severity == Severity.Error);
        return hasError ? DocumentStatus.HataliEksik : DocumentStatus.Onaylandi;
    }

    private static ValidationResult ReadFailure(string message) => new()
    {
        RuleName = "Belge okuma",
        FieldName = "Dosya",
        IsValid = false,
        Severity = Severity.Error,
        Message = message
    };
}