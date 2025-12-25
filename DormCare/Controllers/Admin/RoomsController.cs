using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;

namespace DormCare.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RoomsController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var rooms = _db.Rooms.ToList();
            return View(rooms);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Room room)
        {
            if (!ModelState.IsValid) return View(room);
            try
            {
                _db.Rooms.Add(room);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save room: " + ex.Message);
                return View(room);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var room = _db.Rooms.Find(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Room room)
        {
            if (!ModelState.IsValid) return View(room);
            _db.Rooms.Update(room);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var room = _db.Rooms.Find(id);
            if (room == null) return NotFound();
            var occupied = _db.Students.Count(s => s.RoomId == id);
            ViewData["Occupied"] = occupied;
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var room = _db.Rooms.Find(id);
            if (room == null) return NotFound();
            var occupied = _db.Students.Count(s => s.RoomId == id);
            if (occupied > 0)
            {
                TempData["Error"] = "Cannot delete a room that still has assigned students.";
                return RedirectToAction("Delete", new { id });
            }
            _db.Rooms.Remove(room);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
