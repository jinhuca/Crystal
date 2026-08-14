using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class BuiltInPointingDeviceTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = new byte[0x07 - 4]; // offsets 0x04..0x06
        payload[0x00] = (byte)PointingDeviceType.TouchPad;
        payload[0x01] = (byte)PointingDeviceInterface.Usb;
        payload[0x02] = 3; // three buttons

        var table  = MakeTable(MakeStructure(21, 0x0070, payload, new[] { "Touchpad" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var dev = smbios.BuiltInPointingDevices.FirstOrDefault();
        Assert.NotNull(dev);
        Assert.Equal(PointingDeviceType.TouchPad, dev!.DeviceType);
        Assert.Equal(PointingDeviceInterface.Usb, dev.Interface);
        Assert.Equal(3, dev.NumberOfButtons);
    }

    [Fact]
    public void Decode_ShortStructure_UsesDefaultsForMissingFields()
    {
        // Minimal structure with only DeviceType present (length = 5 -> payload 1 byte)
        var payload = new byte[0x05 - 4];
        payload[0x00] = (byte)PointingDeviceType.Mouse;

        var table  = MakeTable(MakeStructure(21, 0x0071, payload, new string[0]));
        var smbios = SmbiosTable.FromRawTableData(table);

        var dev = smbios.BuiltInPointingDevices.First();
        Assert.Equal(PointingDeviceType.Mouse, dev.DeviceType);
        Assert.Equal(PointingDeviceInterface.Unknown, dev.Interface);
        Assert.Equal(0, dev.NumberOfButtons);
    }
}
