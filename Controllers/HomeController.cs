﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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