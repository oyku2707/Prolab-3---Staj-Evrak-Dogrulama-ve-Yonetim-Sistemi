using System.Text.RegularExpressions;

namespace StajSistemi.Validation.Rules;

public class AddressRule : ValidationRuleBase
{
    private const string ExpectedFormat = "Mahalle, Cadde/Sokak, Bina No, Daire No, İlçe/İl";
    private const string Example = "Atatürk Mah. Gül Sok. No:5 Daire:3 Kadıköy/İstanbul";

    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private sealed record AddressPart(string Name, Regex Pattern);

    // Şartname (6.1): adres bu parçaları bu sırayla ve eksiksiz içermelidir.
    // (?<![\p{L}]) ve (?![\p{L}]) ifadeleri, aranan kısaltmanın başka bir kelimenin içinde
    // yakalanmasını önler (örn. "Mahmut" içindeki "mah").
    private static readonly AddressPart[] Parts =
    [
        new("Mahalle", new Regex(@"(?<![\p{L}])mah(alle(si)?)?(?![\p{L}])", Options)),
        new("Cadde/Sokak", new Regex(
            @"(?<![\p{L}])(cad(de(si)?)?|cd|sok(ak|a[ğĞg][ıiIİ])?|sk|bulvar[ıiIİ]?|blv)(?![\p{L}])", Options)),
        new("Bina No", new Regex(@"(?<![\p{L}])no\s*[:.]?\s*[0-9]+", Options)),
        new("Daire No", new Regex(@"(?<![\p{L}])(daire|d)\s*[:.]?\s*[0-9]+", Options)),
        // İlçe/İl: adresin sonunda, eğik çizgiyle ayrılmış iki ad (örn. Kadıköy/İstanbul).
        new("İlçe/İl", new Regex(@"\p{L}+(\s\p{L}+)*\s*/\s*\p{L}+(\s\p{L}+)*\s*\.?\s*$", Options)),
    ];

    public override string Name => "Adres";

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        return
        [
            CheckAddress("Address", "İkametgâh adresi", context.Data.Address),
            CheckAddress("CompanyAddress", "İşyeri adresi", context.Data.CompanyAddress),
        ];
    }

    private ValidationResult CheckAddress(string field, string label, string? address)
    {
        var suggestion = $"Beklenen biçim: {ExpectedFormat}. Örnek: {Example}";

        if (string.IsNullOrWhiteSpace(address))
            return Fail(field, $"{label} boş bırakılmış.", suggestion: suggestion);

        var missing = new List<string>();
        var positions = new List<int>();   // bulunan parçaların adres içindeki yeri, beklenen sırayla

        foreach (var part in Parts)
        {
            var match = part.Pattern.Match(address);

            if (match.Success)
                positions.Add(match.Index);
            else
                missing.Add(part.Name);
        }

        // Bulunan parçaların yerleri, beklenen sırada artmalıdır.
        var inOrder = true;
        for (var i = 1; i < positions.Count; i++)
        {
            if (positions[i] <= positions[i - 1])
                inOrder = false;
        }

        var problems = new List<string>();
        if (missing.Count > 0)
            problems.Add($"eksik: {string.Join(", ", missing)}");
        if (!inOrder)
            problems.Add($"sıra hatalı, beklenen sıra: {ExpectedFormat}");

        if (problems.Count == 0)
            return Pass(field);

        return Fail(field,
            $"{label} tam ve doğru sırada değil ({string.Join("; ", problems)}): '{address.Trim()}'.",
            suggestion: suggestion);
    }
}
