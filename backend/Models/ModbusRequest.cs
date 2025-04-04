public class ModbusRequest
{
    public ushort StartAddress { get; set; }  // The starting address of the Modbus register
    public byte Value { get; set; }            // The value to write to the Modbus register (0 or 1)
    public string Alias { get; set; }          // The alias (name) of the device
}
