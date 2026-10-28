using Microsoft.AspNetCore.Mvc;
using StajSistemi.Services;

namespace StajSistemi.Controllers;

public class HomeController : Controller
{
    private readonly ICurrentUserService _currentUser;

    public HomeController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public IActionResult Index()
        => _currentUser.IsAdmin
            ? RedirectToAction("Documents", "Admin")
            : RedirectToAction("Index", "Student");
}