using Microsoft.AspNetCore.Mvc;

namespace ObituaryApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Obituary");
    }
}
