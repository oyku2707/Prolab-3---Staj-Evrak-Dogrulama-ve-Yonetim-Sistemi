namespace StajSistemi.DTOs
{
    public class RejectedRow
    {
        public int LineNumber { get; set; }
        public string Reason { get; set; } = "";
    }

    public class DatasetImportResult
    {
        public int Added { get; set; }
        public int Updated { get; set; }
        public List<RejectedRow> Rejected { get; } = new();
        public string? Error { get; set; } // dosya türü desteklenmiyorsa vb.
    }
}