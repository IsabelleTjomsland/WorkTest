using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Worktest.backend.Controller;
using Xunit;

namespace Worktest.Tests.ControllerTests
{
    /// <summary>
    /// Unit tests for the <see cref="ModbusController"/> class.
    /// </summary>
    public class ModbusControllerTests
    {
        private readonly ModbusController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusControllerTests"/> class.
        /// </summary>
        public ModbusControllerTests()
        {
            _controller = new ModbusController();
        }

        /// <summary>
        /// Tests that ReadRegister returns BadRequest for an invalid start address.
        /// </summary>
        [Fact]
        public async Task ReadRegister_InvalidStartAddress_ReturnsBadRequest()
        {
            // Arrange
            ushort invalidAddress = 99;

            // Act
            var result = await _controller.ReadRegister(invalidAddress);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid start address.", badRequest.Value);
        }

        /// <summary>
        /// Tests that UpdateRegister returns BadRequest when ModbusRequest is null.
        /// </summary>
        [Fact]
        public async Task UpdateRegister_NullDevice_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateRegister(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid device data or start address.", badRequest.Value);
        }

        /// <summary>
        /// Tests that UpdateRegister returns BadRequest for an invalid start address.
        /// </summary>
        [Fact]
        public async Task UpdateRegister_InvalidStartAddress_ReturnsBadRequest()
        {
            // Arrange
            var device = new Worktest.backend.Controller.ModbusRequest
            {
                StartAddress = 99,
                Value = 1,
                Alias = "InvalidDevice"
            };

            // Act
            var result = await _controller.UpdateRegister(device);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid device data or start address.", badRequest.Value);
        }
    }
}
