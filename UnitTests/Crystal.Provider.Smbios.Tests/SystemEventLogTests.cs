using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class SystemEventLogTests
{
    [Fact]
    public void Decode_PopulatesHeaderFields()
    {
        // DSP0134 §7.16: LogAreaLength WORD@0x04, LogHeaderStartOffset WORD@0x06,
        // LogDataStartOffset WORD@0x08, AccessMethod@0x0A, LogStatus@0x0B,
        // LogChangeToken DWORD@0x0C, AccessMethodAddress DWORD@0x10,
        // LogHeaderFormat@0x14, DescriptorCount@0x15, DescriptorLength@0x16.
        // No descriptors -> length 0x17. Payload index = offset - 4.
        var payload = new byte[0x17 - 4];
        payload[0x00] = 0x00; payload[0x01] = 0x10; // LogAreaLength @0x04 = 0x1000
        payload[0x02] = 0x10; payload[0x03] = 0x00; // LogHeaderStartOffset @0x06 = 0x0010
        payload[0x04] = 0x20; payload[0x05] = 0x00; // LogDataStartOffset @0x08 = 0x0020
        payload[0x06] = 0x03;                       // AccessMethod @0x0A
        payload[0x07] = 0x01;                       // LogStatus @0x0B = valid (bit0)
        payload[0x08] = 0x78; payload[0x09] = 0x56; payload[0x0A] = 0x34; payload[0x0B] = 0x12; // ChangeToken @0x0C = 0x12345678
        payload[0x0C] = 0x00; payload[0x0D] = 0x00; payload[0x0E] = 0x0C; payload[0x0F] = 0x00; // AccessMethodAddress @0x10 = 0x000C0000
        payload[0x10] = 0x01;                       // LogHeaderFormat @0x14
        payload[0x11] = 0x00;                       // DescriptorCount @0x15
        payload[0x12] = 0x00;                       // DescriptorLength @0x16

        var table = MakeTable(MakeStructure(15, 0x1500, payload));
        var log = SmbiosTable.FromRawTableData(table).SystemEventLogs.First();

        Assert.Equal((ushort)0x1000, log.LogAreaLength);
        Assert.Equal((ushort)0x0010, log.LogHeaderStartOffset);
        Assert.Equal((ushort)0x0020, log.LogDataStartOffset);
        Assert.Equal((byte)0x03, log.AccessMethod);
        Assert.True(log.IsValid);
        Assert.False(log.IsFull);
        Assert.Equal(0x12345678u, log.LogChangeToken);
        Assert.Equal(0x000C0000u, log.AccessMethodAddress);
        Assert.Equal((byte)0x01, log.LogHeaderFormat);
        Assert.Equal(0, log.SupportedLogTypeDescriptorCount);
        Assert.True(log.LogTypeDescriptors.IsEmpty);
    }

    [Fact]
    public void Decode_FullLogStatus_IsFullTrue()
    {
        var payload = new byte[0x17 - 4];
        payload[0x07] = 0x03; // LogStatus @0x0B = valid | full

        var table = MakeTable(MakeStructure(15, 0x1501, payload));
        var log = SmbiosTable.FromRawTableData(table).SystemEventLogs.First();

        Assert.True(log.IsValid);
        Assert.True(log.IsFull);
    }

    [Fact]
    public void Decode_TypeDescriptors_SlicedByCountAndLength()
    {
        // 2 descriptors of 2 bytes each -> descriptor bytes at 0x17..0x1A,
        // length 0x1B.
        var payload = new byte[0x1B - 4];
        payload[0x11] = 0x02; // DescriptorCount @0x15
        payload[0x12] = 0x02; // DescriptorLength @0x16
        payload[0x13] = 0x01; payload[0x14] = 0x02; // descriptor[0] @0x17
        payload[0x15] = 0x03; payload[0x16] = 0x04; // descriptor[1] @0x19

        var table = MakeTable(MakeStructure(15, 0x1502, payload));
        var log = SmbiosTable.FromRawTableData(table).SystemEventLogs.First();

        Assert.Equal(2, log.SupportedLogTypeDescriptorCount);
        Assert.Equal(2, log.LogTypeDescriptorLength);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, log.LogTypeDescriptors.ToArray());
    }

    [Fact]
    public void Decode_LegacyShortStructure_UsesDefaults()
    {
        // v2.0-era: only through AccessMethod (length 0x0B).
        var payload = new byte[0x0B - 4];
        payload[0x00] = 0x00; payload[0x01] = 0x08; // LogAreaLength @0x04 = 0x0800
        payload[0x06] = 0x01;                       // AccessMethod @0x0A

        var table = MakeTable(MakeStructure(15, 0x1503, payload));
        var log = SmbiosTable.FromRawTableData(table).SystemEventLogs.First();

        Assert.Equal((ushort)0x0800, log.LogAreaLength);
        Assert.Equal((byte)0x01, log.AccessMethod);
        Assert.Equal(0u, log.LogChangeToken);
        Assert.Equal((byte)0, log.LogHeaderFormat);
        Assert.Equal(0, log.SupportedLogTypeDescriptorCount);
    }
}
