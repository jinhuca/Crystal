using Crystal.Provider.Smbios.Structures;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

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
    }

    [Fact]
    public void MemoryDeviceMappedAddress_ExtendedFields_DecodedFromQWordBytes()
    {
        // Structure layout (offsets are absolute within the SMBIOS structure):
        //   0x04 legacy start DWORD, 0x08 legacy end DWORD, 0x0C device handle,
        //   0x0E array-mapped handle, 0x10 partition row, 0x11 interleave position,
        //   0x12 interleaved data depth, 0x13 extended start QWORD, 0x1B extended end QWORD.
        // Formatted area begins at 0x04, so payload index = offset - 4. The end QWORD
        // finishes at 0x22, so the structure length is 0x23 and payload is 0x1F bytes.
        var payload = new byte[0x23 - 4];
        // legacy start/end = 0xFFFFFFFF -> forces extended decode
        payload[0x00] = 0xFF; payload[0x01] = 0xFF; payload[0x02] = 0xFF; payload[0x03] = 0xFF;
        payload[0x04] = 0xFF; payload[0x05] = 0xFF; payload[0x06] = 0xFF; payload[0x07] = 0xFF;
        payload[0x08] = 0x20; payload[0x09] = 0x00; // MemoryDeviceHandle @0x0C = 0x0020
        payload[0x0A] = 0x30; payload[0x0B] = 0x00; // MemoryArrayMappedAddressHandle @0x0E = 0x0030
        payload[0x0C] = 0x01;                       // PartitionRowPosition @0x10
        payload[0x0D] = 0x02;                       // InterleavePosition @0x11
        payload[0x0E] = 0x03;                       // InterleavedDataDepth @0x12
        // extended start QWORD @0x13 (payload index 0x0F) = 0x2000_0000 bytes
        ulong startBytes = 0x20000000UL;
        for (int i = 0; i < 8; i++) payload[0x0F + i] = (byte)(startBytes >> (8 * i));
        // extended end QWORD @0x1B (payload index 0x17)
        ulong endBytes = 0x2000FFFFUL;
        for (int i = 0; i < 8; i++) payload[0x17 + i] = (byte)(endBytes >> (8 * i));

        var table = MakeTable(MakeStructure(20, 0x0200, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var map = smbios.MemoryDeviceMappedAddresses.FirstOrDefault();
        Assert.NotNull(map);
        Assert.Equal((long)(startBytes / 1024), map!.StartAddressKiB);
        Assert.True(map.UsesExtendedAddresses);
        Assert.Equal(startBytes, map.StartAddressBytes);
        Assert.Equal(endBytes, map.EndAddressBytes);
        Assert.Equal((ushort)0x0030, map.MemoryArrayMappedAddressHandle);
        Assert.Equal((byte)0x01, map.PartitionRowPosition);
        Assert.Equal((byte)0x02, map.InterleavePosition);
        Assert.Equal((byte)0x03, map.InterleavedDataDepth);
    }
}
