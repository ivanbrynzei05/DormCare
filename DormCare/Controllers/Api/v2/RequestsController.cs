using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;
using Microsoft.EntityFrameworkCore;

namespace DormCare.Controllers.Api.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RequestsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/v2/requests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Requests.ToListAsync();
            return Ok(items);
        }

        // GET: api/v2/requests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.Requests.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        // POST: api/v2/requests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Request req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            req.CreatedAt = System.DateTime.UtcNow;
            _db.Requests.Add(req);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = req.Id, version = "2.0" }, req);
        }
    }
}
