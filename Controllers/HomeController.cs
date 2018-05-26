using Microsoft.AspNetCore.Mvc;

namespace TTAWeb
{    
    [Controller]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}