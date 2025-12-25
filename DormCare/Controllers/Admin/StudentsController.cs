using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;

namespace DormCare.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public StudentsController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var students = _db.Students.ToList();
            return View(students);
        }

        public IActionResult AssignRoom(int id)
        {
            var student = _db.Students.Find(id);
            if (student == null) return NotFound();
            var rooms = _db.Rooms
                .Select(r => new DormCare.Models.RoomViewModel
                {
                    Id = r.Id,
                    Number = r.Number,
                    Capacity = r.Capacity,
                    Occupied = _db.Students.Count(s => s.RoomId == r.Id)
                })
                .ToList();

            ViewData["Rooms"] = rooms;
            return View(student);
        }

        [HttpPost]
        public IActionResult AssignRoom(int id, int? roomId)
        {
            var student = _db.Students.Find(id);
            if (student == null) return NotFound();
            if (roomId.HasValue)
            {
                var room = _db.Rooms.Find(roomId.Value);
                if (room == null) return NotFound("Room not found");
                var occupied = _db.Students.Count(s => s.RoomId == roomId.Value && s.Id != id);
                if (occupied >= room.Capacity)
                {
                    ModelState.AddModelError("", "Selected room is full");
                    var rooms = _db.Rooms
                        .Select(r => new DormCare.Models.RoomViewModel
                        {
                            Id = r.Id,
                            Number = r.Number,
                            Capacity = r.Capacity,
                            Occupied = _db.Students.Count(s => s.RoomId == r.Id)
                        })
                        .ToList();
                    ViewData["Rooms"] = rooms;
                    return View(student);
                }
            }

            student.RoomId = roomId;
            _db.Students.Update(student);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Evict(int id)
        {
            var student = _db.Students.Find(id);
            if (student == null) return NotFound();
            student.RoomId = null;
            _db.Students.Update(student);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
