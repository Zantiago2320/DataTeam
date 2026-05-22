using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataTeam.Controllers;

[Authorize]
public class DemoUIController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
