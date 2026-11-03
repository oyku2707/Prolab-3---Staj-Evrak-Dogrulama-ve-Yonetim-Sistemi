using StajSistemi.Validation;

namespace StajSistemi.Domain.Interfaces;

public interface IValidationRule
{
    string Name { get; }
    IEnumerable<ValidationResult> Validate(ValidationContext context);
}