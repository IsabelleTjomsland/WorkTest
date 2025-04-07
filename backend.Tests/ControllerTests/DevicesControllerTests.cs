using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Worktest.backend.Models;
using Worktest.backend.Controller;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

/// <summary>
/// Unit tests for the <see cref="DevicesController"/> using an in-memory database.
/// </summary>
namespace  Worktest.Tests.ControllerTests
{
    public class DevicesControllerTests : IDisposable
    {
        private ApplicationDbContext _context;
        private DevicesController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicesControllerTests"/> class.
        /// </summary>
        public DevicesControllerTests()
        {
            _context = GetInMemoryDbContext(); // Fresh context for each test
            _controller = GetController(_context);
        }

        /// <summary>
        /// Creates an in-memory database context for testing.
        /// </summary>
        /// <returns>A new <see cref="ApplicationDbContext"/> instance.</returns>
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "DevicesTestDb")
                .Options;

            return new ApplicationDbContext(options);
        }

        /// <summary>
        /// Creates a new instance of <see cref="DevicesController"/> with the provided context.
        /// </summary>
        /// <param name="context">The database context to use.</param>
        /// <returns>An instance of <see cref="DevicesController"/>.</returns>
        private DevicesController GetController(ApplicationDbContext context)
        {
            return new DevicesController(context);
        }

        /// <summary>
        /// Clears the devices table in the database.
        /// </summary>
        private void ClearDatabase()
        {
            _context.Devices.RemoveRange(_context.Devices);
            _context.SaveChanges();
        }

        /// <summary>
        /// Tests if <see cref="DevicesController.GetDevices"/> returns all devices.
        /// </summary>
        [Fact]
        public async Task GetDevices_ReturnsAllDevices()
        {
            // Arrange
            ClearDatabase();
            _context.Devices.AddRange(new List<Device>
            {
                new Device { Id = 1, Name = "Device 1", Status = true },
                new Device { Id = 2, Name = "Device 2", Status = false }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetDevices() as OkObjectResult;

            // Assert
            var devices = Assert.IsType<List<Device>>(result.Value);
            Assert.Equal(2, devices.Count);
        }

        /// <summary>
        /// Tests if <see cref="DevicesController.AddDevice"/> creates a new device when data is valid.
        /// </summary>
        [Fact]
        public async Task AddDevice_CreatesDevice_WhenValid()
        {
            // Arrange
            ClearDatabase();
            var newDevice = new Device { Name = "New Device", Status = true };

            // Act
            var result = await _controller.AddDevice(newDevice) as CreatedAtActionResult;

            // Assert
            var added = Assert.IsType<Device>(result.Value);
            Assert.Equal("New Device", added.Name);
            Assert.Single(await _context.Devices.ToListAsync());
        }

        /// <summary>
        /// Tests if <see cref="DevicesController.UpdateDevice"/> returns NoContent when update is successful.
        /// </summary>
        [Fact]
        public async Task UpdateDevice_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            ClearDatabase();
            var device = new Device { Id = 1, Name = "Old", Status = false };
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            // Act
            var updated = new Device { Id = 1, Name = "Updated", Status = true };
            var result = await _controller.UpdateDevice(1, updated);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Tests if <see cref="DevicesController.DeleteDevice"/> returns NoContent when deletion is successful.
        /// </summary>
        [Fact]
        public async Task DeleteDevice_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            ClearDatabase();
            var device = new Device { Id = 1, Name = "ToDelete", Status = true };
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteDevice(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Disposes the test context to release resources after each test.
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
