using StajSistemi.Domain.Enums;

namespace StajSistemi.Validation.Rules;

public class EngineerCountRule : ValidationRuleBase
{
    public override string Name => "Mühendis sayısı";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        return [CheckCount(context)];
    }

    private ValidationResult CheckCount(ValidationContext context)
    {
        var count = context.Data.EngineerCount;

        if (count is null)
            return Fail("EngineerCount", "Mühendis sayısı boş bırakılmış.");

        if (count < 1)
            return Fail("EngineerCount", "Mühendis sayısı en az 1 olmalıdır.");

        if (count < 5)
            return Fail("EngineerCount",
                "Mühendis sayısı 5'in altında: lütfen mühendisin mezun olduğu bölümü belirtin.",
                Severity.Warning);

        return Pass("EngineerCount");
    }
}