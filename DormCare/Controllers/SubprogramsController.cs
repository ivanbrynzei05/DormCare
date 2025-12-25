using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormCare.Controllers
{
    [Authorize]
    public class SubprogramsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        // Subprogram 1: Reverse input text
        [HttpGet]
        public IActionResult Sub1()
        {
            ViewData["Description"] = "Reverses input text and shows the result.";
            return View();
        }

        [HttpPost]
        public IActionResult Sub1(string input)
        {
            var output = string.IsNullOrEmpty(input) ? string.Empty : new string(input.Reverse().ToArray());
            ViewData["Description"] = "Reverses input text and shows the result.";
            ViewData["Output"] = output;
            return View();
        }

        // Subprogram 2: Uppercase converter
        [HttpGet]
        public IActionResult Sub2()
        {
            ViewData["Description"] = "Converts input text to uppercase.";
            return View();
        }

        [HttpPost]
        public IActionResult Sub2(string input)
        {
            var output = string.IsNullOrEmpty(input) ? string.Empty : input.ToUpperInvariant();
            ViewData["Description"] = "Converts input text to uppercase.";
            ViewData["Output"] = output;
            return View();
        }

        // Subprogram 3: Word count
        [HttpGet]
        public IActionResult Sub3()
        {
            ViewData["Description"] = "Counts words in the provided text.";
            return View();
        }

        [HttpPost]
        public IActionResult Sub3(string input)
        {
            var count = 0;
            if (!string.IsNullOrWhiteSpace(input))
            {
                count = input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
            }
            ViewData["Description"] = "Counts words in the provided text.";
            ViewData["Output"] = count.ToString();
            return View();
        }
    }
}
