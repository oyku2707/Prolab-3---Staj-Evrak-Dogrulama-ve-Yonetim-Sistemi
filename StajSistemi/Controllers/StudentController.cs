using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Models;
using StajSistemi.Services;

namespace StajSistemi.Controllers;

// TODO: Kişi 1'in girişi gelince [Authorize(Roles = "ROLE_STUDENT")] eklenecek.
public class StudentController : Controller
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public StudentController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _currentUser.UserId;

        var documents = await _db.Documents
            .Where(d => d.StudentId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new DocumentRowViewModel
            {
                Id = d.Id,
                FileType = d.FileType,
                UploadedAt = d.UploadedAt,
                Status = d.Status
            })
            .ToListAsync();

        return View(documents);
    }
}