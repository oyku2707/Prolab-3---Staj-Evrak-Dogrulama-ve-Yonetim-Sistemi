using StajSistemi.Domain.Entities;

namespace StajSistemi.Models;

public class UploadViewModel
{
    public IFormFile? File { get; set; }
    public Ek16Data Entered { get; set; } = new();
}