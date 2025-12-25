using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;
using Microsoft.EntityFrameworkCore;

namespace DormCare.Controllers.Api.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/students")]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public StudentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Students.Include(s => s.User).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();
            return Ok(s);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DormCare.Models.Student student)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.Students.Add(student);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = student.Id, version = "2.0" }, student);
        }
    }
}
