using cursova.Api.Data;
using cursova.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cursova.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            return Ok(trip);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var trips = await _context.Trips.ToListAsync();
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
                return NotFound();

            return Ok(trip);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Trip updated)
        {
            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
                return NotFound();

            trip.Name = updated.Name;
            trip.Description = updated.Description;
            trip.StartTrip = updated.StartTrip;
            trip.EndTrip = updated.EndTrip;
            trip.StartDate= updated.StartDate;
            trip.Type = updated.Type;

            await _context.SaveChangesAsync();

            return Ok(trip);
        }
        [HttpPatch("{id}/done")]
        public async Task<IActionResult> ChangeIsDone(int id, [FromBody] bool isDone)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return NotFound();

            trip.IsDone = isDone;
            await _context.SaveChangesAsync();

            return Ok(trip);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
                return NotFound();

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
