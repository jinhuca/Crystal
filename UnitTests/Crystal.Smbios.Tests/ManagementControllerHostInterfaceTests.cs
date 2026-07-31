using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class ManagementControllerHostInterfaceTests
{
    [Fact]
    public void Decode_NoProtocolRecords()
    {
        var payload = new byte[]
        {
            0x02,       // InterfaceType = Kcs
            0x02,       // InterfaceTypeSpecificDataLength = 2
            0xCA, 0x02, // InterfaceTypeSpecificData
            0x00,       // Number of Protocol Records = 0
        };

        var table = MakeTable(MakeStructure(42, 0x0220, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementControllerHostInterfaces[0];
        Assert.Equal(ManagementControllerHostInterfaceType.Kcs, m.InterfaceType);
        Assert.Equal(2, m.InterfaceTypeSpecificData.Count);
        Assert.Equal((byte)0xCA, m.InterfaceTypeSpecificData[0]);
        Assert.Equal(0, m.ProtocolRecordCount);
        Assert.Empty(m.ProtocolRecords);
    }

    [Fact]
    public void Decode_WithProtocolRecords()
    {
        var payload = new byte[]
        {
            0x40,             // InterfaceType = NetworkHostInterface
            0x00,             // InterfaceTypeSpecificDataLength = 0
            0x01,             // Number of Protocol Records = 1
            0x04,             // Protocol Type = RedfishOverIp
            0x02,             // Protocol-Type-Specific Data Length = 2
            0xAA, 0xBB,       // Protocol-Type-Specific Data
        };

        var table = MakeTable(MakeStructure(42, 0x0221, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementControllerHostInterfaces[0];
        Assert.Equal(ManagementControllerHostInterfaceType.NetworkHostInterface, m.InterfaceType);
        Assert.Empty(m.InterfaceTypeSpecificData);
        Assert.Equal(1, m.ProtocolRecordCount);
        Assert.Single(m.ProtocolRecords);

        var rec = m.ProtocolRecords[0];
        Assert.Equal(ManagementControllerProtocolType.RedfishOverIp, rec.ProtocolType);
        Assert.Equal(2, rec.ProtocolTypeSpecificDataLength);
        Assert.Equal(new byte[] { 0xAA, 0xBB }, rec.ProtocolTypeSpecificData);
    }

    [Fact]
    public void Decode_UnknownInterfaceType_NullableEnumIsNull()
    {
        var payload = new byte[] { 0x7A, 0x00, 0x00 };
        var table = MakeTable(MakeStructure(42, 0x0222, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.ManagementControllerHostInterfaces[0];
        Assert.Equal((byte)0x7A, m.InterfaceTypeRaw);
        Assert.Null(m.InterfaceType);
    }

    [Fact]
    public void Decode_MultipleProtocolRecords_WalksVariableLengthData()
    {
        var payload = new byte[]
        {
            0x02,             // InterfaceType = Kcs
            0x00,             // InterfaceTypeSpecificDataLength = 0
            0x02,             // Number of Protocol Records = 2
            0x02, 0x00,       // Record 0: IPMI, 0 data bytes
            0x7F, 0x03,       // Record 1: unknown protocol type, 3 data bytes
            0x0A, 0x0B, 0x0C, // Record 1 data
        };

        var table = MakeTable(MakeStructure(42, 0x0223, payload));
        var m = SmbiosTable.FromRawTableData(table).ManagementControllerHostInterfaces[0];

        Assert.Equal(2, m.ProtocolRecordCount);
        Assert.Equal(2, m.ProtocolRecords.Count);
        Assert.Equal(ManagementControllerProtocolType.Ipmi, m.ProtocolRecords[0].ProtocolType);
        Assert.Empty(m.ProtocolRecords[0].ProtocolTypeSpecificData);
        Assert.Null(m.ProtocolRecords[1].ProtocolType); // 0x7F is not a defined enum value
        Assert.Equal((byte)0x7F, m.ProtocolRecords[1].ProtocolTypeRaw);
        Assert.Equal(new byte[] { 0x0A, 0x0B, 0x0C }, m.ProtocolRecords[1].ProtocolTypeSpecificData);
    }
}
