using System;
using System.Collections.Generic;
using System.Net.Sockets;
using NModbus;

namespace Worktest.backend.Services
{
    /// <summary>
    /// A service that handles Modbus communication tasks, such as reading and writing holding registers.
    /// </summary>
    public class ModbusService
    {
        private readonly string _ipAddress = "127.0.0.1";  // Example IP
        private readonly int _port = 502;  // Standard Modbus TCP port

        // Timeout for TCP connections 
        private readonly int _timeout = 5000; // 5 seconds

        /// <summary>
        /// Reads holding registers from a Modbus slave.
        /// </summary>
        /// <param name="startAddress">The starting address of the holding registers to read.</param>
        /// <param name="count">The number of holding registers to read.</param>
        /// <returns>A list of ushort values representing the holding registers read from the Modbus slave.</returns>
        public List<ushort> ReadHoldingRegisters(ushort startAddress, ushort count)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Set timeouts for connection and data transmission
                    client.ReceiveTimeout = _timeout;
                    client.SendTimeout = _timeout;
                    client.Connect(_ipAddress, _port);

                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);  // Create a Modbus master instance

                    // Read holding registers from Modbus slave
                    var result = master.ReadHoldingRegisters(1, startAddress, count);  // Slave address 1
                    return new List<ushort>(result);  // Convert array to list and return
                }
            }
            catch (Exception ex)
            {
                // Log error and return an empty list if failure occurs
                Console.WriteLine($"Error reading holding registers: {ex.Message}");
                return new List<ushort>();
            }
        }

        /// <summary>
        /// Writes values to holding registers in a Modbus slave.
        /// </summary>
        /// <param name="startAddress">The starting address of the holding registers to write to.</param>
        /// <param name="values">An array of values to write to the holding registers.</param>
        /// <returns>True if the write operation is successful; otherwise, false.</returns>
        public bool WriteHoldingRegisters(ushort startAddress, ushort[] values)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Set timeouts for connection and data transmission
                    client.ReceiveTimeout = _timeout;
                    client.SendTimeout = _timeout;
                    client.Connect(_ipAddress, _port);

                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);  // Create a Modbus master instance

                    // Write multiple holding registers to Modbus slave
                    master.WriteMultipleRegisters(1, startAddress, values);  // Slave address 1

                    return true;  // Return true if successful
                }
            }
            catch (Exception ex)
            {
                // Log error and return false if failure occurs
                Console.WriteLine($"Error writing holding registers: {ex.Message}");
                return false;
            }
        }
    }
}
