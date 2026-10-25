using StajSistemi.Domain.Enums;
namespace StajSistemi.Validation;

public class ValidationResult
{
        public string RuleName { get; init; } = "";
        public string FieldName { get; init; } = "";
        public bool IsValid { get; init; }
        public Severity Severity { get; init; } = Severity.Error;
        public string Message { get; init; } = "";
        public string? Suggestion { get; init; }
}