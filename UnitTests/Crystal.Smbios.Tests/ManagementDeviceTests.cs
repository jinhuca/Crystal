using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class ManagementDeviceTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = new byte[]
        {
            1,                // Description string #1
            0x03,             // Type = Lm75
            0x90, 0x02, 0x00, 0x00, // Address = 0x00000290
            0x05,             // AddressType = Smbus
        };

        var table = MakeTable(MakeStructure(34, 0x01C0, payload, new[] { "LM75 Thermal Sensor" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementDevices[0];
        Assert.Equal("LM75 Thermal Sensor", m.Description);
        Assert.Equal(ManagementDeviceType.Lm75, m.Type);
        Assert.Equal(0x00000290u, m.Address);
        Assert.Equal(ManagementDeviceAddressType.Smbus, m.AddressType);
    }
}
