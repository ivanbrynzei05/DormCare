using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;

namespace DormCare.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RequestsController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var requests = _db.Requests.ToList();
            return View(requests);
        }

        // A simple page that demonstrates using the API (client-side)
        [HttpGet]
        public IActionResult ApiClient()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangeStatus(int id, string status)
        {
            var req = _db.Requests.Find(id);
            if (req == null) return NotFound();
            // Allow only two statuses: New and Done
            var normalized = (status ?? string.Empty).Trim();
            if (normalized != "New" && normalized != "Done")
            {
                ModelState.AddModelError("", "Invalid status");
                return RedirectToAction("Index");
            }
            req.Status = normalized;
            _db.Requests.Update(req);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
