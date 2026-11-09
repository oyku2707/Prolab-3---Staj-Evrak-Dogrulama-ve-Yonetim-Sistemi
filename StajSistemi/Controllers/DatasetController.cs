using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajSistemi.DTOs;
using StajSistemi.Services.Dataset;

namespace StajSistemi.Controllers
{
    [Authorize(Roles = "ROLE_ADMIN")]
    public class DatasetController : Controller
    {
        private readonly DatasetImportService _importService;

        public DatasetController(DatasetImportService importService)
        {
            _importService = importService;
        }

        // GET: /Dataset/Upload
        [HttpGet]
        public IActionResult Upload()
        {
            return View((DatasetImportResult?)null);
        }

        // POST: /Dataset/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen bir dosya seçiniz.");
                return View((DatasetImportResult?)null);
            }

            string extension = Path.GetExtension(file.FileName);

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportAsync(stream, extension);

            return View(result);
        }
    }
}