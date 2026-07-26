using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class ManagementControllerHostInterfaceTests
{
    [Fact]
    public void Decode_NoTrailingBytes()
    {
        var payload = new byte[]
        {
            0x02,       // InterfaceType = Kcs
            0x02,       // InterfaceTypeSpecificDataLength = 2
            0xCA, 0x02, // InterfaceTypeSpecificData
        };

        var table = MakeTable(MakeStructure(42, 0x0220, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementControllerHostInterfaces[0];
        Assert.Equal(ManagementControllerHostInterfaceType.Kcs, m.InterfaceType);
        Assert.Equal(2, m.InterfaceTypeSpecificData.Count);
        Assert.Equal((byte)0xCA, m.InterfaceTypeSpecificData[0]);
        Assert.Empty(m.TrailingBytes);
    }

    [Fact]
    public void Decode_WithTrailingProtocolRecordBytes()
    {
        var payload = new byte[]
        {
            0x40,       // InterfaceType = NetworkHostInterface
            0x00,       // InterfaceTypeSpecificDataLength = 0
            0x01, 0x02, // trailing protocol-record bytes, undecoded
        };

        var table = MakeTable(MakeStructure(42, 0x0221, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementControllerHostInterfaces[0];
        Assert.Equal(ManagementControllerHostInterfaceType.NetworkHostInterface, m.InterfaceType);
        Assert.Empty(m.InterfaceTypeSpecificData);
        Assert.Equal(2, m.TrailingBytes.Count);
        Assert.Equal((byte)0x01, m.TrailingBytes[0]);
        Assert.Equal((byte)0x02, m.TrailingBytes[1]);
    }

    [Fact]
    public void Decode_UnknownInterfaceType_NullableEnumIsNull()
    {
        var payload = new byte[] { 0x7A, 0x00 };
        var table = MakeTable(MakeStructure(42, 0x0222, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementControllerHostInterfaces[0];
        Assert.Equal((byte)0x7A, m.InterfaceTypeRaw);
        Assert.Null(m.InterfaceType);
    }
}
