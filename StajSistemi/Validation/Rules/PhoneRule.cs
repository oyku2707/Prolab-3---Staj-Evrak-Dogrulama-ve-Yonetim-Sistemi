using System.Text.RegularExpressions;

namespace StajSistemi.Validation.Rules;

public class PhoneRule : ValidationRuleBase
{
    private static readonly Regex PhonePattern =
        new(@"^\+90 [0-9]{3} [0-9]{3} [0-9]{2} [0-9]{2}$");

    public override string Name => "Telefon biçimi";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var d = context.Data;
        return
        [
            Check("StudentPhone", "Öğrenci telefonu", d.StudentPhone),
            Check("CompanyPhone", "İşyeri telefonu", d.CompanyPhone),
            Check("EmployerPhone", "İşveren telefonu", d.EmployerPhone)
        ];
    }

    private ValidationResult Check(string field, string label, string? value)
    {
        var phone = value?.Trim();

        if (string.IsNullOrWhiteSpace(phone))
            return Fail(field, $"{label} boş bırakılmış.");

        if (!PhonePattern.IsMatch(phone))
            return Fail(field, $"{label} +90 xxx xxx xx xx biçiminde olmalıdır.");

        return Pass(field);
    }
}