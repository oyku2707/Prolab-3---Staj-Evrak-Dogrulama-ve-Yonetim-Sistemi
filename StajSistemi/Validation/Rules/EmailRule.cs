using System.Text.RegularExpressions;

namespace StajSistemi.Validation.Rules;

public class EmailRule : ValidationRuleBase
{
    private static readonly Regex EmailPattern =
        new(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$");

    public override string Name => "E-posta biçimi";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var d = context.Data;
        return
        [
            Check("StudentEmail", "Öğrenci e-postası", d.StudentEmail),
            Check("CompanyEmail", "İşyeri e-postası", d.CompanyEmail),
            Check("EmployerEmail", "İşveren e-postası", d.EmployerEmail)
        ];
    }

    private ValidationResult Check(string field, string label, string? value)
    {
        var email = value?.Trim();

        if (string.IsNullOrWhiteSpace(email))
            return Fail(field, $"{label} boş bırakılmış.");

        if (!EmailPattern.IsMatch(email))
            return Fail(field, $"{label} geçerli bir e-posta biçiminde olmalıdır (örn. ad@alanadi.uzanti).");

        return Pass(field);
    }
}