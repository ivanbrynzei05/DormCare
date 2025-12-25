using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DormCare.Data;

namespace DormCare.Controllers.Student
{
    [Authorize(Roles = "Student")]
    public class StudentHomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public StudentHomeController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
            var student = _db.Students.FirstOrDefault(s => s.UserId == userId);
            if (student == null) return View(null);
            var room = student.RoomId.HasValue ? _db.Rooms.Find(student.RoomId.Value) : null;
            ViewData["Room"] = room;
            return View(student);
        }
    }
}
