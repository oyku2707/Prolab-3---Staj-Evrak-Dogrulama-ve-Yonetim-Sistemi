using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.Domain.Interfaces;
using UglyToad.PdfPig;

namespace StajSistemi.Services.Extractors;

public sealed class PdfFieldExtractor : IDocumentFieldExtractor
{
    private const double LeftValueX = 190;  
    private const double RightLabelX = 285; 
    private const double RightValueX = 430;  
    private const double RowTol = 4;         

    private sealed record Word(string Text, double X, double Top);
    private sealed record Row(double Top, List<Word> Words);
    private sealed record Field(char Col, string Key, string Name);

    public bool Supports(string extension) =>
        extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public Ek16Data Extract(string filePath)
    {
        using var pdf = PdfDocument.Open(filePath);
        var page = pdf.GetPage(1);   

        var words = page.GetWords()
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .Select(w =>
            {
                var first = w.Letters[0];
                var top = page.Height - first.StartBaseLine.Y - 0.9 * first.PointSize;
                return new Word(w.Text, w.BoundingBox.Left, top);
            })
            .ToList();
        if (words.Count == 0)
            throw new InvalidDataException("PDF metin içermiyor; taranmış PDF için OCR gerekir.");

        var rows = ToRows(words);

        var yStudent = FindRow(rows, "TCKIMLIKNO");
        var yCompany = FindRow(rows, "STAJYAPILANYERIN");
        var yEmployer = FindRow(rows, "ISVERENVEYAYETKILININ");
        if (yStudent == null || yCompany == null || yEmployer == null)
            throw new InvalidDataException("PDF okundu ancak Ek-16 şablonu tanınamadı.");
        var yEnd = FindRow(rows, "OGRENCININONAYI", after: yEmployer.Value) ?? double.MaxValue;

        var s = ReadSection(words, yStudent.Value - 8, yCompany.Value - 5, new[]
        {
            new Field('L', "TCKIMLIKNO", "tc"),
            new Field('L', "ADISOYADI", "ad"),
            new Field('L', "DOGUMTARIHI", "dogum"),
            new Field('L', "OGRENCINUMARASI", "no"),
            new Field('L', "IKAMETGAH", "adres"),
            new Field('R', "BOLUMU", "bolum"),
            new Field('R', "DAHAONCE", "onceki"),
            new Field('R', "OGRETIMYILI", "yil"),
            new Field('R', "EPOSTAADRESI", "iletisim"),
            new Field('R', "SAGLIKYARDIM", "saglik"),
        });

        var c = ReadSection(words, yCompany.Value + 3, yEmployer.Value - 5, new[]
        {
            new Field('L', "ADI", "ad"),
            new Field('L', "HIZMETALANI", "hizmet"),
            new Field('L', "TELEFONNO", "tel"),
            new Field('L', "FAKSNO", "faks"),
            new Field('L', "EPOSTAADRESI", "eposta"),
            new Field('L', "WEBADRESI", "web"),
            new Field('L', "MUHENDISSAYISI", "muh"),
            new Field('L', "ADRESBILGISI", "adres"),
            new Field('R', "YURTDISI", "yurtdisi"),
            new Field('R', "STAJBASLAMA", "bas"),
            new Field('R', "STAJBITIS", "bit"),
            new Field('R', "STAJGUNSAYISI", "gun"),
            new Field('R', "HAFTASONU", "hs"),
            new Field('R', "CALISANSAYISI", "calisan"),
            new Field('R', "UCRETALIYOR", "ucret"),
        }, wideField: "adres");

        var e = ReadSection(words, yEmployer.Value + 3, yEnd - 5, new[]
        {
            new Field('L', "ADISOYADI", "ad"),
            new Field('L', "GOREVUNVANI", "unvan"),
            new Field('L', "EPOSTAADRESI", "eposta"),
            new Field('L', "TELEFONNO", "tel"),
        }, leftOnly: true);

        var data = new Ek16Data
        {
            StudentFullName = Get(s, "ad"),
            BirthDate = ParseDate(Get(s, "dogum")),
            StudentNo = Get(s, "no"),
            Address = Get(s, "adres"),
            Department = Get(s, "bolum"),
            PreviousInternshipDays = ParseInt(Get(s, "onceki")),
            AcademicYear = Get(s, "yil"),

            CompanyName = Get(c, "ad"),
            ServiceArea = Get(c, "hizmet"),
            CompanyPhone = Get(c, "tel"),
            CompanyFax = Get(c, "faks"),
            CompanyEmail = Get(c, "eposta", noSpaces: true),
            CompanyWeb = Get(c, "web", noSpaces: true),
            EngineerCount = ParseInt(Get(c, "muh")),
            CompanyAddress = Get(c, "adres"),
            StartDate = ParseDate(Get(c, "bas")),
            EndDate = ParseDate(Get(c, "bit")),
            InternshipDays = ParseInt(Get(c, "gun")),
            EmployeeCount = ParseInt(Get(c, "calisan")),
            IsAbroad = ParseBool(Get(c, "yurtdisi")),
            WorksOnWeekends = ParseBool(Get(c, "hs")),
            IsPaid = ParseBool(Get(c, "ucret")),

            EmployerFullName = Get(e, "ad"),
            EmployerTitle = Get(e, "unvan"),
            EmployerEmail = Get(e, "eposta", noSpaces: true),
            EmployerPhone = Get(e, "tel"),
        };

        var tcParts = Get(s, "tc")?.Split('/', 2, StringSplitOptions.TrimEntries);
        if (tcParts is { Length: > 0 })
        {
            data.TcNo = tcParts[0].Replace(" ", "");
            if (tcParts.Length == 2 && tcParts[1].Length > 0)
            {
                var n = Normalize(tcParts[1]);
                data.Nationality = n is "TC" or "TURK" or "TURKIYE" ? Nationality.TC : Nationality.Yabanci;
            }
        }

        var contact = Get(s, "iletisim")?.Split('/', 2, StringSplitOptions.TrimEntries);
        if (contact is { Length: > 0 })
        {
            data.StudentEmail = contact[0].Replace(" ", "");   
            if (contact.Length == 2 && contact[1].Length > 0) data.StudentPhone = contact[1];
        }

        var health = Regex.Match(Get(s, "saglik") ?? "", @"([1-6])\s*-\s*[☒☑✔✓xX]");
        if (health.Success) data.HealthAidStatus = int.Parse(health.Groups[1].Value);

        if (string.IsNullOrWhiteSpace(data.StudentFullName) &&
            string.IsNullOrWhiteSpace(data.TcNo) &&
            string.IsNullOrWhiteSpace(data.StudentNo))
            throw new InvalidDataException("PDF okunabildi ancak doldurulmuş Ek-16 öğrenci alanları bulunamadı.");

        ReadSignaturesAndApprovals(page, words, yEmployer.Value, yEnd, data);

        return data;
    }

    private static readonly string[] ApprovalLabels = { "ADI", "SOYADI", "TARIH", "IMZA" };

    // İşveren imza/kaşe ve öğrenci/danışman onay bayraklarını doldurur.
    // Hata olursa bayraklar false kalır; asıl alanların okunması bozulmaz.
    private static void ReadSignaturesAndApprovals(
        UglyToad.PdfPig.Content.Page page, List<Word> words, double yEmployer, double yEnd, Ek16Data data)
    {
        if (yEnd == double.MaxValue) return;   // "ÖĞRENCİNİN ONAYI" satırı bulunamadıysa bölgeler belirsiz

        try
        {
            // Her görselin orta noktası; Y, sayfanın üstünden aşağı doğru ölçülür (kelimelerin Top değeriyle aynı yön).
            var images = page.GetImages()
                .Select(i => (MidX: (i.Bounds.Left + i.Bounds.Right) / 2,
                              MidY: page.Height - (i.Bounds.Top + i.Bounds.Bottom) / 2))
                .ToList();

            // İşveren bölümüne düşen herhangi bir görsel imza ve kaşe sayılır 
            var hasEmployerImage = images.Any(i => i.MidY >= yEmployer && i.MidY < yEnd);
            data.EmployerHasSignature = hasEmployerImage;
            data.EmployerHasStamp = hasEmployerImage;

            // "ÖĞRENCİNİN ONAYI" başlık satırının altı: sol yarı öğrenci, sağ yarı danışman.
            var splitX = page.Width / 2;
            var below = words.Where(w => w.Top > yEnd + RowTol).ToList();

            data.StudentApprovalFilled =
                images.Any(i => i.MidY > yEnd && i.MidX < splitX) ||
                HasLeftoverText(below.Where(w => w.X < splitX), cutAfter: "ARZEDERIM");

            data.AdvisorApprovalFilled =
                images.Any(i => i.MidY > yEnd && i.MidX >= splitX) ||
                HasLeftoverText(below.Where(w => w.X >= splitX), cutAfter: null);
        }
        catch (Exception)
        {
           
        }
    }

    // Standart metin ("... arz ederim") ve Adı/Soyadı/Tarih/İmza etiketleri çıkarıldıktan sonra yazı kalıyor mu?
    private static bool HasLeftoverText(IEnumerable<Word> words, string? cutAfter)
    {
        var kept = ToRows(words.Where(w => !ApprovalLabels.Contains(Normalize(w.Text))))
            .SelectMany(r => r.Words);
        var text = Normalize(string.Concat(kept.Select(w => w.Text)));

        if (cutAfter != null)
        {
            var i = text.IndexOf(cutAfter, StringComparison.Ordinal);
            if (i < 0) return false;   
            text = text[(i + cutAfter.Length)..];
        }
        return text.Length > 0;
    }

    private static List<Row> ToRows(IEnumerable<Word> words)
    {
        var rows = new List<Row>();
        foreach (var w in words.OrderBy(w => w.Top))
        {
            var last = rows.LastOrDefault();
            if (last != null && Math.Abs(last.Top - w.Top) <= RowTol)
                last.Words.Add(w);
            else
                rows.Add(new Row(w.Top, new List<Word> { w }));
        }
        return rows.Select(r => new Row(r.Top, r.Words.OrderBy(w => w.X).ToList())).ToList();
    }

    private static string RowKey(Row row) => Normalize(string.Concat(row.Words.Select(w => w.Text)));

    private static double? FindRow(List<Row> rows, string key, double after = double.MinValue)
    {
        foreach (var r in rows)
            if (r.Top > after && RowKey(r).StartsWith(key, StringComparison.Ordinal)) return r.Top;
        return null;
    }
    private static Dictionary<string, List<string>> ReadSection(
        List<Word> all, double y0, double y1, Field[] fields,
        string? wideField = null, bool leftOnly = false)
    {
        var sec = all.Where(w => w.Top >= y0 && w.Top < y1).ToList();
        var result = fields.ToDictionary(f => f.Name, _ => new List<string>());

        var labels = new List<(char Col, double Top, string Name)>();
        foreach (var (col, lo, hi) in new[] { ('L', 0.0, LeftValueX), ('R', RightLabelX, RightValueX) })
        {
            foreach (var row in ToRows(sec.Where(w => w.X >= lo && w.X < hi)))
            {
                var key = RowKey(row);
                var match = fields.FirstOrDefault(fl => fl.Col == col && key.StartsWith(fl.Key, StringComparison.Ordinal));
                if (match != null) labels.Add((col, row.Top, match.Name));
            }
        }
        double? wideTop = wideField == null ? null :
            labels.Where(l => l.Name == wideField).Select(l => (double?)(l.Top - 6)).FirstOrDefault();

        foreach (var (col, lo) in new[] { ('L', LeftValueX), ('R', RightValueX) })
        {
            var mine = labels.Where(l => l.Col == col).ToList();
            if (mine.Count == 0) continue;

            var valueWords = sec.Where(w => w.X >= lo).Where(w =>
            {
                if (col == 'R') return true;
                if (leftOnly) return w.X < 310;                        // "İmza / Kaşe" yazısını alma
                return w.X < RightLabelX || (wideTop != null && w.Top >= wideTop);
            });

            foreach (var row in ToRows(valueWords))
            {
                string? best = null; double bestD = double.MaxValue;
                foreach (var l in mine)
                {
                    var d = Math.Abs(row.Top - l.Top);
                    if (d <= RowTol) d = 0;
                    else if (l.Top < row.Top) d -= 3;
                    if (d < bestD) { bestD = d; best = l.Name; }
                }
                if (best != null)
                    result[best].Add(string.Join(" ", row.Words.Select(w => w.Text)));
            }
        }
        return result;
    }
    private static string? Get(Dictionary<string, List<string>> section, string name, bool noSpaces = false)
    {
        if (!section.TryGetValue(name, out var parts) || parts.Count == 0) return null;
        var text = string.Join(noSpaces ? "" : " ", parts).Trim();
        if (noSpaces) text = text.Replace(" ", "");
        return text.Length == 0 ? null : text;
    }
    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var sb = new StringBuilder();
        foreach (var ch in text.Replace('ı', 'I').Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToUpperInvariant(ch));
        }
        return sb.ToString();
    }
    private static int? ParseInt(string? value) =>
        int.TryParse(value?.Trim(), out var n) ? n : null;

    private static DateTime? ParseDate(string? value) =>
        DateTime.TryParseExact(value?.Trim(),
            new[] { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
            CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out var date) ? date : null;

    private static bool? ParseBool(string? value) => Normalize(value) switch
    {
        "E" or "EVET" => true,
        "H" or "HAYIR" => false,
        _ => null
    };
}