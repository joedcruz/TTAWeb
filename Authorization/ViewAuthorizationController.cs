using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTAWeb
{
    [Controller]
    public class ViewAuthorizationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [FormIdentityAuthorize("Page1")] 
        [Route("page1")]
        public IActionResult Page1()
        {
            return View("Views/Home/Page1.cshtml");
        }

        [FormIdentityAuthorize("Page2")]
        [Route("page2")]
        public IActionResult Page2()
        {
            return View("Views/Home/Page2.cshtml");
        }
    }
}
