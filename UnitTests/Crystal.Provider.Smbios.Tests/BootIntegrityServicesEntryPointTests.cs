using Xunit;
using Crystal.Provider.Smbios.Structures;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class BootIntegrityServicesEntryPointTests
{
    [Fact]
    public void Decode_PopulatesChecksumAndEntryPoints()
    {
        var payload = new byte[0x1C - 4];
        payload[0x00] = 0xAB; // Checksum
        // Reserved1 (0x05), Reserved2 word (0x06-0x07) left zero
        payload[0x04] = 0x34; payload[0x05] = 0x12; payload[0x06] = 0x00; payload[0x07] = 0xF0; // BisEntry16 = F000:1234
        payload[0x08] = 0x00; payload[0x09] = 0x00; payload[0x0A] = 0x01; payload[0x0B] = 0x00; // BisEntry32 = 0x00010000
        // Reserved3 (qword) + Reserved4 (dword) left zero

        var table = MakeTable(MakeStructure(31, 0x01A0, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var b = smbios.BootIntegrityServicesEntryPoints[0];
        Assert.Equal((byte)0xAB, b.Checksum);
        Assert.Equal((ushort)0xF000, b.BisEntry16Segment);
        Assert.Equal((ushort)0x1234, b.BisEntry16Offset);
        Assert.Equal(0x00010000u, b.BisEntry32Address);
    }
}
