namespace StajSistemi.Helpers;

/// <summary>
/// Telefonu şartnamedeki "+90 xxx xxx xx xx" biçimine çevirir (PhoneRule ile aynı biçim).
/// </summary>
public static class PhoneNormalizer
{
    /// <summary>
    /// "5321112233", "05321112233", "905321112233", "+90 532 111 22 33" gibi girdileri kabul eder.
    /// Çevrilemiyorsa false döner.
    /// </summary>
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = "";
        var digits = new string((input ?? "").Where(c => c >= '0' && c <= '9').ToArray());

        if (digits.Length == 12 && digits.StartsWith("90")) digits = digits[2..];
        else if (digits.Length == 11 && digits.StartsWith("0")) digits = digits[1..];

        if (digits.Length != 10) return false;

        normalized = $"+90 {digits[..3]} {digits.Substring(3, 3)} {digits.Substring(6, 2)} {digits.Substring(8, 2)}";
        return true;
    }
}
