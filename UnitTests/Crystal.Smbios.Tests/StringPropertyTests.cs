using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class StringPropertyTests
{
    [Fact]
    public void Decode_KnownPropertyId_MapsToEnum()
    {
        var payload = new byte[]
        {
            0x01, 0x00, // PropertyIdRaw = DevicePath
            1,          // PropertyValue string #1
            0x11, 0x00, // ParentHandle = 0x0011
        };

        var table = MakeTable(MakeStructure(46, 0x0260, payload, new[] { "PciRoot(0x0)/Pci(0x1,0x0)" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var p = smbios.StringProperties[0];
        Assert.Equal(StringPropertyId.DevicePath, p.PropertyId);
        Assert.Equal("PciRoot(0x0)/Pci(0x1,0x0)", p.PropertyValue);
        Assert.Equal((ushort)0x0011, p.ParentHandle);
    }

    [Fact]
    public void Decode_UnknownPropertyId_NullableEnumIsNull()
    {
        var payload = new byte[] { 0x00, 0xC0, 0, 0x01, 0x00 }; // 0xC000 = OEM-specific range

        var table = MakeTable(MakeStructure(46, 0x0261, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var p = smbios.StringProperties[0];
        Assert.Equal((ushort)0xC000, p.PropertyIdRaw);
        Assert.Null(p.PropertyId);
    }
}
