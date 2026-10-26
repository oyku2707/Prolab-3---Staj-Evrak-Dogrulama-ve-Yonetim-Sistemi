using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public class ValidationResultRecord
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string RuleName { get; set; } = "";
    public string FieldName { get; set; } = "";
    public bool IsValid { get; set; }
    public Severity Severity { get; set; } = Severity.Error;
    public string Message { get; set; } = "";
    public string? Suggestion { get; set; }
}
