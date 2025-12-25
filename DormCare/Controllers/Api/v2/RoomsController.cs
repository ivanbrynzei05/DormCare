using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;
using Microsoft.EntityFrameworkCore;

namespace DormCare.Controllers.Api.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RoomsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Rooms.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.Rooms.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Room room)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = room.Id, version = "2.0" }, room);
        }
    }
}
