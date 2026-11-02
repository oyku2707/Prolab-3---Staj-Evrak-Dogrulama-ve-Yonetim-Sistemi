using System.IO;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.Services;

namespace StajSistemi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly CsvImportService _csvImportService;

        public StudentController(CsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        [HttpPost("import-csv")]
        public IActionResult ImportCsv()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "students.csv");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("students.csv dosyası Data klasöründe bulunamadı.");
            }

            _csvImportService.ImportStudents(filePath);

            return Ok("CSV dosyasındaki öğrenciler başarıyla veritabanına aktarıldı.");
        }
    }
}