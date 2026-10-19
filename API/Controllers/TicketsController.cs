using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class TicketsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
