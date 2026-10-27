using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Validation;

public abstract class ValidationRuleBase : IValidationRule
{
    public abstract string Name { get; }
    public abstract ValidationResult Validate(ValidationContext context);

    protected ValidationResult Pass(string field) => new()
        { RuleName = Name, FieldName = field, IsValid = true };

    protected ValidationResult Fail(string field, string message,
        Severity severity = Severity.Error, string? suggestion = null) => new()
        { RuleName = Name, FieldName = field, IsValid = false,
          Message = message, Severity = severity, Suggestion = suggestion };
}