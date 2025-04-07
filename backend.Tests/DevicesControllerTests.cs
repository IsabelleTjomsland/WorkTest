using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Worktest.backend.Models;
using Worktest.backend.Controller;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Backend.Tests.Controllers
{public class DevicesControllerTests : IDisposable
{
    private ApplicationDbContext _context;
    private DevicesController _controller;

    // Constructor that initializes the context and controller
    public DevicesControllerTests()
    {
        _context = GetInMemoryDbContext(); // Fresh context for each test
        _controller = GetController(_context);
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "DevicesTestDb") // Unique database for each test
            .Options;

        return new ApplicationDbContext(options);
    }

    private DevicesController GetController(ApplicationDbContext context)
    {
        return new DevicesController(context);
    }

    // Ensure clean database state before each test
    private void ClearDatabase()
    {
        _context.Devices.RemoveRange(_context.Devices);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetDevices_ReturnsAllDevices()
    {
        // Arrange
        ClearDatabase(); // Clear previous data
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
        Assert.Equal(2, devices.Count);  // Expecting 2 devices
    }

    [Fact]
    public async Task AddDevice_CreatesDevice_WhenValid()
    {
        // Arrange
        ClearDatabase(); // Ensure no existing devices
        var newDevice = new Device { Name = "New Device", Status = true };

        // Act
        var result = await _controller.AddDevice(newDevice) as CreatedAtActionResult;

        // Assert
        var added = Assert.IsType<Device>(result.Value);
        Assert.Equal("New Device", added.Name);
        Assert.Single(await _context.Devices.ToListAsync());  // Expecting 1 device in the DB
    }

    [Fact]
    public async Task UpdateDevice_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        ClearDatabase(); // Ensure no existing devices
        var device = new Device { Id = 1, Name = "Old", Status = false };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        // Act
        var updated = new Device { Id = 1, Name = "Updated", Status = true };
        var result = await _controller.UpdateDevice(1, updated);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDevice_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        ClearDatabase(); // Ensure no existing devices
        var device = new Device { Id = 1, Name = "ToDelete", Status = true };
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteDevice(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    // Method to clean up resources after each test
    public void Dispose()
    {
        _context?.Dispose();
    }
}
}
