using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;  // 👈 Add this
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

        // GET: Read register (Requires authentication)
        [HttpGet("read-register")]
        public async Task<ActionResult<ModbusRequest>> ReadRegister(ushort startAddress)
        {
            try
            {
                if (!_registerAliases.ContainsKey(startAddress))
                {
                    return BadRequest("Invalid start address.");
                }

                using (TcpClient client = new TcpClient(_ipAddress, _port))
                {
                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);
                    ushort[] values = master.ReadHoldingRegisters(1, startAddress, 1);
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

        // PUT: Update register (Requires authentication)
        [HttpPut("update-register")]
        public async Task<ActionResult> UpdateRegister([FromBody] ModbusRequest device)
        {
            if (device == null || !_registerAliases.ContainsKey(device.StartAddress))
            {
                return BadRequest("Invalid device data or start address.");
            }

            try
            {
                Console.WriteLine($"Updating Modbus: Address {device.StartAddress}, Value {device.Value}, Alias {device.Alias}");

                using (TcpClient client = new TcpClient(_ipAddress, _port))
                {
                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);
                    master.WriteSingleRegister(1, device.StartAddress, device.Value);
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
}
