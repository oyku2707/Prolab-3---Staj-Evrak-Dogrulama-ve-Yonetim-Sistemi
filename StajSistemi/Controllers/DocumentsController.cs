using Microsoft.AspNetCore.Mvc;
using StajSistemi.Models;
using StajSistemi.Services;
using StajSistemi.Services.Documents;

namespace StajSistemi.Controllers;

// TODO: Kişi 1'in girişi gelince [Authorize] eklenecek.
public class DocumentsController : Controller
{
    private readonly IDocumentService _documents;
    private readonly ICurrentUserService _currentUser;

    public DocumentsController(IDocumentService documents, ICurrentUserService currentUser)
    {
        _documents = documents;
        _currentUser = currentUser;
    }

    [HttpGet]
    public IActionResult Upload() => View(new UploadViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadViewModel model)
    {
        if (_currentUser.UserId is not int studentId)
            return Unauthorized();

        var result = await _documents.UploadAsync(studentId, model.File, model.Entered);

        if (!result.Success)
        {
            // Yanlış uzantı, boş dosya, büyük dosya: uyarı göster, formu geri ver.
            ViewBag.Error = result.Error;
            return View(model);
        }

        return RedirectToAction("Report", new { id = result.DocumentId });
    }
}