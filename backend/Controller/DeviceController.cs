using Microsoft.AspNetCore.Authorization; // 🔒 För att skydda API:et
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Worktest.backend.Models;

namespace Worktest.backend.Controller
{
    [Route("api/devices")]
    [ApiController]
    [Authorize] // 🔒 Skyddar alla endpoints - kräver inloggning
    public class DevicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DevicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔍 GET: api/devices (Hämta alla enheter)
        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _context.Devices.ToListAsync();
            return Ok(devices);
        }

        // 🔍 GET: api/devices/{id} (Hämta en enhet)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            return Ok(device);
        }

        // ➕ POST: api/devices (Lägg till en ny enhet)
        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] Device newDevice)
        {
            if (newDevice == null || string.IsNullOrWhiteSpace(newDevice.Name))
                return BadRequest("Invalid device data");

            _context.Devices.Add(newDevice);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevice), new { id = newDevice.Id }, newDevice);
        }

        // ✏️ PUT: api/devices/{id} (Uppdatera en enhet)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] Device updatedDevice)
        {
            if (id != updatedDevice.Id)
                return BadRequest("ID mismatch");

            var existingDevice = await _context.Devices.FindAsync(id);
            if (existingDevice == null)
                return NotFound();

            existingDevice.Name = updatedDevice.Name;
            existingDevice.Status = updatedDevice.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ❌ DELETE: api/devices/{id} (Ta bort en enhet)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
