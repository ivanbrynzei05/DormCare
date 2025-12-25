using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DormCare.Data;
using DormCare.Models;

namespace DormCare.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RequestsController(ApplicationDbContext db) { _db = db; }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(string text, string type)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
            var student = _db.Students.FirstOrDefault(s => s.UserId == userId);
            if (student == null) return NotFound("Student record not found");

            var req = new Request { StudentId = student.Id, Text = text, Type = type, Status = "New" };
            _db.Requests.Add(req);
            _db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
    }
}
