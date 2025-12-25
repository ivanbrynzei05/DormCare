using Microsoft.AspNetCore.Mvc;
using DormCare.Data;
using DormCare.Models;
using Microsoft.EntityFrameworkCore;

namespace DormCare.Controllers.Api.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RequestsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/v1/requests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // v1: older shape - do not include CreatedAt field
            var items = await _db.Requests.Select(r => new
            {
                r.Id,
                r.StudentId,
                r.Text,
                r.Type,
                r.Status
            }).ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var r = await _db.Requests.FindAsync(id);
            if (r == null) return NotFound();
            return Ok(new { r.Id, r.StudentId, r.Text, r.Type, r.Status });
        }

        // POST: api/v1/requests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Request req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            req.CreatedAt = System.DateTime.UtcNow;
            _db.Requests.Add(req);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = req.Id, version = "1.0" }, new { req.Id, req.StudentId, req.Text, req.Type, req.Status });
        }
    }
}
