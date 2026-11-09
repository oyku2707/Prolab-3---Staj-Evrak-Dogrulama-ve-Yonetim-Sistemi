namespace StajSistemi.Models;

public class GoogleFormViewModel
{
    public int DocumentId { get; set; }
    public string? Url { get; set; }
    public string? Error { get; set; }
    public bool Valid { get; set; }
}