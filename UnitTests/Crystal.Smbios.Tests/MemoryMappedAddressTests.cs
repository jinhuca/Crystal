using Crystal.Smbios.Structures;
using System.Linq;
using Xunit;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class MemoryMappedAddressTests
{
    [Fact]
    public void MemoryArrayMappedAddress_LegacyFields_DecodedAsKiB()
    {
        var payload = new byte[0x1C - 4];
        // start = 0x00001000 (KiB), end = 0x00001FFF (KiB)
        payload[0x00] = 0x00; payload[0x01] = 0x10; payload[0x02] = 0x00; payload[0x03] = 0x00; // 0x001000
        payload[0x04] = 0xFF; payload[0x05] = 0x1F; payload[0x06] = 0x00; payload[0x07] = 0x00; // 0x001FFF
        payload[0x0C] = 0x10; payload[0x0D] = 0x00; // MemoryArrayHandle = 0x0010

        var table = MakeTable(MakeStructure(19, 0x0100, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var map = smbios.MemoryArrayMappedAddresses.FirstOrDefault();
        Assert.NotNull(map);
        Assert.Equal(0x1000L, map!.StartAddressKiB);
        Assert.Equal(0x1FFF, map.EndAddressKiB);
        Assert.False(map.UsesExtendedAddresses);
        Assert.Equal(0x1000UL * 1024UL, map.StartAddressBytes);
        Assert.Null(map.InterleavePosition);
        Assert.Null(map.InterleaveGranularityBytes);
    }

    [Fact]
    public void MemoryDeviceMappedAddress_ExtendedFields_DecodedFromQWordBytes()
    {
        // Construct a structure where legacy fields are 0xFFFFFFFF and extended QWORDs exist
        var payload = new byte[0x20 - 4];
        // legacy start/end = 0xFFFFFFFF
        payload[0x00] = 0xFF; payload[0x01] = 0xFF; payload[0x02] = 0xFF; payload[0x03] = 0xFF;
        payload[0x04] = 0xFF; payload[0x05] = 0xFF; payload[0x06] = 0xFF; payload[0x07] = 0xFF;
        // memory device handle at 0x0C
        payload[0x08] = 0x20; payload[0x09] = 0x00;
        // extended start address (QWORD at 0x10) = 0x2000_0000 bytes -> KiB = 0x2000_0000 / 1024
        // Place little-endian bytes at offset 0x10 (payload index 0x0C)
        ulong startBytes = 0x20000000UL;
        for (int i = 0; i < 8; i++) payload[0x0C + i] = (byte)(startBytes >> (8 * i));

        var table = MakeTable(MakeStructure(20, 0x0200, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var map = smbios.MemoryDeviceMappedAddresses.FirstOrDefault();
        Assert.NotNull(map);
        Assert.Equal((long)(startBytes / 1024), map!.StartAddressKiB);
        Assert.True(map.UsesExtendedAddresses);
        Assert.Equal(startBytes, map.StartAddressBytes);
    }
}
