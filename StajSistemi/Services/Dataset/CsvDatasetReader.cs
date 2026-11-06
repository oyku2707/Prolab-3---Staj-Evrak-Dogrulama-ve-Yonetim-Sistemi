using System.Text;

namespace StajSistemi.Services.Dataset
{
    public class CsvDatasetReader : IDatasetReader
    {
        public bool Supports(string extension) =>
            extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);

        public IEnumerable<DatasetRow> Read(Stream stream)
        {
            // Türkçe karakterler için UTF8
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? header = reader.ReadLine(); // başlık satırı atlanır
            if (header == null)
                yield break;

            // Excel'den Türkçe kaydedilen CSV ';' kullanabilir
            char delimiter = header.Contains(';') && !header.Contains(',') ? ';' : ',';

            int lineNumber = 1;
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                // Boş satır ve '#' ile başlayan yorum satırlarını atla
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Satır sonundaki "# TEST 1" gibi yorumları temizle
                string cleanLine = line.Split('#')[0].Trim();

                yield return new DatasetRow
                {
                    LineNumber = lineNumber,
                    Fields = SplitLine(cleanLine, delimiter)
                };
            }
        }

        // Tırnak içindeki virgülleri bölmeyen basit ayrıştırıcı
        private static string[] SplitLine(string line, char delimiter)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                    sb.Append(c);
            }
            result.Add(sb.ToString().Trim());
            return result.ToArray();
        }
    }
}