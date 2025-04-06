using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NModbus;

namespace Worktest.backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // 👈 Require authentication for all endpoints in this controller
    public class ModbusController : ControllerBase
    {
        private readonly string _ipAddress = "127.0.0.1";  // IP address for Modbus server
        private readonly int _port = 502;  // Modbus TCP port

        // Dictionary to store register aliases
        private readonly Dictionary<ushort, string> _registerAliases = new Dictionary<ushort, string>
        {
            { 1, "Alarm" },
            { 2, "Pump" },
            { 3, "TemperatureSensor" },
            { 4, "PressureSensor" },
            { 5, "FlowSensor" },
            { 6, "LevelSensor" },
            { 7, "VibrationSensor" },
            { 8, "HumiditySensor" },
            { 9, "Status" },
            { 10, "Control" },
        };

        /// <summary>
        /// Reads the Modbus register value for a specific start address.
        /// </summary>
        /// <param name="startAddress">The starting address of the Modbus register.</param>
        /// <returns>An ActionResult containing the Modbus register data, or a BadRequest if an error occurs.</returns>
        [HttpGet("read-register")]
        public async Task<ActionResult<ModbusRequest>> ReadRegister(ushort startAddress)
        {
            try
            {
                // Validate start address
                if (!_registerAliases.ContainsKey(startAddress))
                {
                    return BadRequest("Invalid start address.");
                }

                // Adjust the start address for Modbus (subtract 1 to match ModbusHD addressing)
                ushort adjustedStartAddress = (ushort)(startAddress - 1);

                // Connect to Modbus server and read register value
                using (TcpClient client = new TcpClient(_ipAddress, _port))
                {
                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);
                    ushort[] values = master.ReadHoldingRegisters(1, adjustedStartAddress, 1);
                    byte value = (byte)values[0];

                    return Ok(new ModbusRequest
                    {
                        StartAddress = startAddress,
                        Value = value,
                        Alias = _registerAliases[startAddress]
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error reading Modbus register: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the Modbus register with a new value.
        /// </summary>
        /// <param name="device">The Modbus request data containing the register address, value, and alias.</param>
        /// <returns>An ActionResult indicating the success or failure of the update operation.</returns>
        [HttpPut("update-register")]
        public async Task<ActionResult> UpdateRegister([FromBody] ModbusRequest device)
        {
            // Validate the device data and start address
            if (device == null || !_registerAliases.ContainsKey(device.StartAddress))
            {
                return BadRequest("Invalid device data or start address.");
            }

            try
            {
                // Log the update attempt
                Console.WriteLine($"Updating Modbus: Address {device.StartAddress}, Value {device.Value}, Alias {device.Alias}");

                // Adjust the start address for Modbus (subtract 1 to match ModbusHD addressing)
                ushort adjustedStartAddress = (ushort)(device.StartAddress - 1);

                // Connect to Modbus server and update the register value
                using (TcpClient client = new TcpClient(_ipAddress, _port))
                {
                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);
                    master.WriteSingleRegister(1, adjustedStartAddress, device.Value);

                    // Update the alias for the register
                    _registerAliases[device.StartAddress] = device.Alias;

                    return Ok("Device updated successfully");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating Modbus register: {ex.Message}");
            }
        }
    }

    // Define ModbusRequest class if it doesn't exist
    public class ModbusRequest
    {
        public ushort StartAddress { get; set; }
        public byte Value { get; set; }
        public string Alias { get; set; }
    }
}
