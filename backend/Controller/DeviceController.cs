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

        /// <summary>
        /// Retrieves all devices from the database.
        /// </summary>
        /// <returns>An IActionResult containing the list of devices.</returns>
        // 🔍 GET: api/devices (Hämta alla enheter)
        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _context.Devices.ToListAsync();
            return Ok(devices);
        }

        /// <summary>
        /// Retrieves a specific device by its ID.
        /// </summary>
        /// <param name="id">The ID of the device to retrieve.</param>
        /// <returns>An IActionResult containing the device if found, or a 404 if not found.</returns>
        // 🔍 GET: api/devices/{id} (Hämta en enhet)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            return Ok(device);
        }

        /// <summary>
        /// Adds a new device to the database.
        /// </summary>
        /// <param name="newDevice">The device to be added.</param>
        /// <returns>An IActionResult containing the newly created device, with a 201 status code.</returns>
        // ➕ POST: api/devices (Lägg till en ny enhet)
        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] Device newDevice)
        {
            // Validate device data
            if (newDevice == null || string.IsNullOrWhiteSpace(newDevice.Name))
                return BadRequest("Invalid device data");

            // Add the new device to the database
            _context.Devices.Add(newDevice);
            await _context.SaveChangesAsync();

            // Return the created device with a 201 status code
            return CreatedAtAction(nameof(GetDevice), new { id = newDevice.Id }, newDevice);
        }

        /// <summary>
        /// Updates an existing device with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the device to update.</param>
        /// <param name="updatedDevice">The updated device data.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        // ✏️ PUT: api/devices/{id} (Uppdatera en enhet)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] Device updatedDevice)
        {
            // Check if the ID in the URL matches the ID in the body
            if (id != updatedDevice.Id)
                return BadRequest("ID mismatch");

            // Find the device to update
            var existingDevice = await _context.Devices.FindAsync(id);
            if (existingDevice == null)
                return NotFound();

            // Update device properties
            existingDevice.Name = updatedDevice.Name;
            existingDevice.Status = updatedDevice.Status;

            // Save changes
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific device by its ID.
        /// </summary>
        /// <param name="id">The ID of the device to delete.</param>
        /// <returns>An IActionResult indicating the result of the deletion operation.</returns>
        // ❌ DELETE: api/devices/{id} (Ta bort en enhet)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            // Find the device to delete
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            // Remove the device from the database
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            // Return a NoContent status code
            return NoContent();
        }
    }
}
