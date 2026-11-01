using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;

namespace StajSistemi.Services.Extractors;

public class DocxFieldExtractor : IDocumentFieldExtractor
{
    private static readonly CultureInfo Tr = new("tr-TR");

    public bool Supports(string fileExtension) =>
        fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public Ek16Data Extract(string filePath)
    {
        var data = new Ek16Data();

        using var wp = WordprocessingDocument.Open(filePath, false);
        var tables = wp.MainDocumentPart!.Document.Body!.Descendants<Table>().ToList();

        var student = FindTable(tables, "KİMLİK NO");
        if (student != null) ReadStudent(student, data);

        var company = FindTable(tables, "HİZMET ALANI");
        if (company != null) ReadCompany(company, data);

        var employer = FindTable(tables, "GÖREV / UNVANI");
        if (employer != null) ReadEmployer(employer, data);

        var approvals = FindTable(tables, "BELGE ÜZERİNDEKİ BİLGİLERİN");
        if (approvals != null) ReadApprovals(approvals, data);
       
        return data;
    }

    // ---------- bölümler ----------

    private void ReadStudent(Table table, Ek16Data data)
    {
        var (tc, nationality) = SplitSlash(TextAfterLabel(table, "T.C. KİMLİK NO"));
        data.TcNo = tc;
        data.Nationality = ParseNationality(nationality);

        data.StudentFullName = TextAfterLabel(table, "ADI SOYADI");
        data.BirthDate = ParseDate(TextAfterLabel(table, "DOĞUM TARİHİ"));
        data.StudentNo = TextAfterLabel(table, "ÖĞRENCİ NUMARASI");
        data.Address = TextAfterLabel(table, "İKAMET");
        data.Department = TextAfterLabel(table, "BÖLÜMÜ");
        data.PreviousInternshipDays = ParseInt(TextAfterLabel(table, "DAHA ÖNCE YAPTIĞI STAJ GÜN SAYISI"));
        data.AcademicYear = TextAfterLabel(table, "ÖĞRETİM YILI");

        var (email, phone) = SplitSlash(TextAfterLabel(table, "E-POSTA ADRESİ"));
        data.StudentEmail = email;
        data.StudentPhone = phone;

        data.HealthAidStatus = ParseHealthAid(TextAfterLabel(table, "SAĞLIK YARDIM STATÜSÜ"));
    }

    private void ReadCompany(Table t, Ek16Data data)
    {
        data.CompanyName = TextAfterLabel(t, "Adı");
        data.ServiceArea = TextAfterLabel(t, "Hizmet Alanı");
        data.CompanyPhone = TextAfterLabel(t, "Telefon No");
        data.CompanyFax = TextAfterLabel(t, "Faks No");
        data.CompanyEmail = TextAfterLabel(t, "E-Posta Adresi");
        data.CompanyWeb = TextAfterLabel(t, "Web Adresi");
        data.EngineerCount = ParseInt(TextAfterLabel(t, "Mühendis Sayısı"));
        data.EmployeeCount = ParseInt(TextAfterLabel(t, "Çalışan Sayısı"));
        data.CompanyAddress = TextAfterLabel(t, "Adres Bilgisi");
        data.IsAbroad = ParseYesNo(TextAfterLabel(t, "Yurtdışı Stajımı"));
        data.StartDate = ParseDate(TextAfterLabel(t, "Staj Başlama Tarihi"));
        data.EndDate = ParseDate(TextAfterLabel(t, "Staj Bitiş Tarihi"));
        data.InternshipDays = ParseInt(TextAfterLabel(t, "Staj Gün Sayısı"));
        data.WorksOnWeekends = ParseYesNo(TextAfterLabel(t, "Hafta Sonu Çalışılıyor mu"));
        data.IsPaid = ParseYesNo(TextAfterLabel(t, "Ücret Alıyor mu"));
    }

    private void ReadEmployer(Table t, Ek16Data data)
    {
        data.EmployerFullName = TextAfterLabel(t, "Adı Soyadı");
        data.EmployerTitle = TextAfterLabel(t, "Görev / Unvanı");
        data.EmployerEmail = TextAfterLabel(t, "E-Posta adresi");
        data.EmployerPhone = TextAfterLabel(t, "Telefon No");

        var hasMark = t.Descendants<Drawing>().Any() || t.Descendants<Picture>().Any();
        data.EmployerHasSignature = hasMark;
        data.EmployerHasStamp = hasMark;
    }

    private void ReadApprovals(Table t, Ek16Data data)
    {
        var cells = t.Elements<TableRow>().First().Elements<TableCell>().ToList();
        if (cells.Count > 0) data.StudentApprovalFilled = HasFilledFields(cells[0], "arz ederim");
        if (cells.Count > 1) data.AdvisorApprovalFilled = HasFilledFields(cells[1], null);
    }

    // ---------- yardımcılar ----------

    private static string Normalize(string? text)
{
    if (string.IsNullOrWhiteSpace(text)) return "";

    var decomposed = text.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();
    foreach (var ch in decomposed)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            continue;

        var c = ch == '\u0131' ? 'I' : ch;

        if (char.IsLetterOrDigit(c))
            sb.Append(char.ToUpperInvariant(c));
    }
    return sb.ToString();
}

    private static Table? FindTable(List<Table> tables, string label)
    {
        var key = Normalize(label);
        return tables
            .Where(t => Normalize(t.InnerText).Contains(key, StringComparison.Ordinal))
            .OrderBy(t => t.Descendants<Table>().Count())
            .FirstOrDefault();
    }

    private static TableCell? CellAfterLabel(Table table, string label)
    {
        var key = Normalize(label);
        foreach (var row in table.Elements<TableRow>())
        {
            var cells = row.Elements<TableCell>().ToList();
            for (int i = 0; i < cells.Count - 1; i++)
                if (Normalize(cells[i].InnerText).StartsWith(key, StringComparison.Ordinal))
                    return cells[i + 1];
        }
        return null;
    }

    private static string? TextAfterLabel(Table table, string label)
    {
        var text = CellAfterLabel(table, label)?.InnerText.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static (string? First, string? Second) SplitSlash(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, null);
        var parts = text.Split('/', 2, StringSplitOptions.TrimEntries);
        var first = string.IsNullOrWhiteSpace(parts[0]) ? null : parts[0];
        var second = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : null;
        return (first, second);
    }

    private static DateTime? ParseDate(string? text)
    {
        string[] formats = { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy" };
        if (text is null) return null;
        return DateTime.TryParseExact(text.Trim(), formats, Tr, DateTimeStyles.None, out var d) ? d : null;
    }

    private static int? ParseInt(string? text) =>
        int.TryParse(text?.Trim(), out var n) ? n : null;

    private static bool? ParseYesNo(string? text)
    {
        var n = Normalize(text);
        if (n is "E" or "EVET") return true;
        if (n is "H" or "HAYIR") return false;
        return null;
    }

    private static Nationality? ParseNationality(string? text)
    {
        var n = Normalize(text);
        if (n == "") return null;
        return n is "TC" or "TURK" or "TURKIYE" ? Nationality.TC : Nationality.Yabanci;
    }

    private static int? ParseHealthAid(string? text)
    {
        if (text is null) return null;
        var m = Regex.Match(text, @"([1-6])\s*-\s*[☒☑✔✓xX]");
        return m.Success ? int.Parse(m.Groups[1].Value) : null;
    }

    private static bool HasFilledFields(TableCell cell, string? afterText)
    {
        if (cell.Descendants<Drawing>().Any() || cell.Descendants<Picture>().Any())
            return true;

        var text = cell.InnerText;
        if (afterText != null)
        {
            var idx = text.IndexOf(afterText, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) text = text[(idx + afterText.Length)..];
        }

        foreach (var label in new[] { "Soyadı", "Adı", "Tarih", "İmza" })
            text = text.Replace(label, "", StringComparison.OrdinalIgnoreCase);

        text = text.Replace(":", "");
        return !string.IsNullOrWhiteSpace(text);
    }
}