namespace StajSistemi.Services.Dataset
{
    public class DatasetRow
    {
        public int LineNumber { get; set; }
        public string[] Fields { get; set; } = Array.Empty<string>();
    }

    public interface IDatasetReader
    {
        bool Supports(string extension);
        IEnumerable<DatasetRow> Read(Stream stream);
    }
}