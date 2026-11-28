namespace StajSistemi.Validation.Rules;

public class ApprovalSectionRule : ValidationRuleBase
{
    public override string Name => "İşveren ve onay alanları";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var d = context.Data;

        return
        [
            // Şartname (6.4): işveren kısmı tamamen dolu olmalı, kaşe ve imza dahil.
            // İşverenin e-postası ve telefonu EmailRule ve PhoneRule tarafından zaten denetlenir.
            CheckFilled("EmployerFullName", "İşveren adı soyadı", d.EmployerFullName),
            CheckFilled("EmployerTitle", "İşveren görev/unvanı", d.EmployerTitle),
            CheckPresent("EmployerHasSignature", "İşveren imzası", d.EmployerHasSignature),
            CheckPresent("EmployerHasStamp", "İşveren kaşesi", d.EmployerHasStamp),

            // Şartname (6.4): öğrenci ve staj danışmanı onay kısmı boş olmalı.
            CheckEmpty("StudentApprovalFilled", "Öğrenci onay kısmı", d.StudentApprovalFilled),
            CheckEmpty("AdvisorApprovalFilled", "Staj danışmanı onay kısmı", d.AdvisorApprovalFilled),
        ];
    }

    private ValidationResult CheckFilled(string field, string label, string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Fail(field, $"{label} boş bırakılmış.")
            : Pass(field);

    private ValidationResult CheckPresent(string field, string label, bool present) =>
        present
            ? Pass(field)
            : Fail(field, $"{label} bulunamadı.");

    private ValidationResult CheckEmpty(string field, string label, bool filled) =>
        filled
            ? Fail(field, $"{label} boş olmalıdır, ancak doldurulmuş.")
            : Pass(field);
}
