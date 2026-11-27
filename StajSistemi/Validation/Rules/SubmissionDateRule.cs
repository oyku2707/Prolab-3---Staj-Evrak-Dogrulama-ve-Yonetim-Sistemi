using System.Globalization;

namespace StajSistemi.Validation.Rules;

public class SubmissionDateRule : ValidationRuleBase
{
    // Şartname (6.3): belge, staj başlangıç tarihinden en az 30 gün önce teslim edilmelidir.
    private const int MinDaysBeforeStart = 30;

    // Çok eski yıllar (örn. 0001) tarihten gün çıkarırken hata verir, bu yüzden hesaplanmaz.
    private const int MinYear = 2000;

    public override string Name => "Teslim tarihi";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var startDate = context.Data.StartDate;

        // Başlangıç tarihi okunamadıysa 30 günü hesaplayacak bir değer yoktur.
        if (startDate is null || startDate.Value.Year < MinYear)
            return [];

        return [CheckSubmission(context.SubmissionDate, startDate.Value)];
    }

    private ValidationResult CheckSubmission(DateTime submissionDate, DateTime startDate)
    {
        // Yalnızca gün sayılır, saat önemsizdir. Teslim günü, son teslim günü dahil kabul edilir.
        var submitted = submissionDate.Date;
        var deadline = startDate.Date.AddDays(-MinDaysBeforeStart);

        if (submitted <= deadline)
            return Pass("SubmissionDate");

        // Bugün teslim edilen bir belge için staj en erken hangi tarihte başlayabilir?
        var earliestStart = submitted.AddDays(MinDaysBeforeStart);

        return Fail("SubmissionDate",
            $"Belge staj başlangıcından ({FormatDate(startDate)}) en az {MinDaysBeforeStart} gün önce " +
            $"teslim edilmemiş: teslim tarihi {FormatDate(submitted)}, son teslim tarihi {FormatDate(deadline)}.",
            suggestion:
                $"Bu staj başlangıcı için belgenin en geç {FormatDate(deadline)} tarihine kadar teslim edilmesi gerekirdi. " +
                $"{FormatDate(submitted)} tarihinde teslim edilen bir belge için staj en erken {FormatDate(earliestStart)} tarihinde başlayabilir.");
    }

    private static string FormatDate(DateTime date) =>
        date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
