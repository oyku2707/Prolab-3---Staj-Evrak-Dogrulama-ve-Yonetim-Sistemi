using StajSistemi.Domain.Interfaces;
using StajSistemi.Validation;

namespace StajSistemi.Services;

public class ValidationService
{
    private readonly IEnumerable<IValidationRule> _rules;

    public ValidationService(IEnumerable<IValidationRule> rules)
    {
        _rules = rules;
    }

    public List<ValidationResult> RunRules(ValidationContext context)
    {
        var results = new List<ValidationResult>();

        foreach (var rule in _rules)
            results.AddRange(rule.Validate(context));

        return results;
    }
}