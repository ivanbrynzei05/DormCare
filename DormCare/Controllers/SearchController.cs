using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;
using System.Linq;

namespace DormCare.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SearchController(ApplicationDbContext db) { _db = db; }

        public IActionResult Index(string fromDate, string toDate, string statuses, string nameStartsWith, string nameEndsWith, string rooms)
        {
            DateTime? from = null, to = null;
            if (System.DateTime.TryParse(fromDate, out var f)) from = f;
            if (System.DateTime.TryParse(toDate, out var t)) to = t;

            var statusList = (statuses ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            var roomList = (rooms ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            var query = from req in _db.Requests
                        join stu in _db.Students on req.StudentId equals stu.Id
                        join usr in _db.Users on stu.UserId equals usr.Id
                        join room in _db.Rooms on stu.RoomId equals room.Id into roomJoin
                        from roomLeft in roomJoin.DefaultIfEmpty()
                        select new
                        {
                            Request = req,
                            Student = stu,
                            User = usr,
                            Room = roomLeft
                        };

            if (from.HasValue) query = query.Where(x => x.Request.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(x => x.Request.CreatedAt <= to.Value);
            if (statusList.Any()) query = query.Where(x => statusList.Contains(x.Request.Status));
            if (!string.IsNullOrWhiteSpace(nameStartsWith)) query = query.Where(x => x.User.FullName.StartsWith(nameStartsWith));
            if (!string.IsNullOrWhiteSpace(nameEndsWith)) query = query.Where(x => x.User.FullName.EndsWith(nameEndsWith));
            if (roomList.Any()) query = query.Where(x => x.Room != null && roomList.Contains(x.Room.Number));

            var results = query.Select(x => new SearchResultViewModel
            {
                RequestId = x.Request.Id,
                Text = x.Request.Text,
                Type = x.Request.Type,
                Status = x.Request.Status,
                CreatedAt = x.Request.CreatedAt,
                StudentId = x.Student.Id,
                StudentName = x.User.FullName,
                RoomNumber = x.Room != null ? x.Room.Number : null
            }).ToList();

            var vm = new SearchPageViewModel
            {
                Results = results
            };
            return View(vm);
        }
    }

    public class SearchPageViewModel
    {
        public List<SearchResultViewModel> Results { get; set; } = new List<SearchResultViewModel>();
    }

    public class SearchResultViewModel
    {
        public int RequestId { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string RoomNumber { get; set; }
    }
}
