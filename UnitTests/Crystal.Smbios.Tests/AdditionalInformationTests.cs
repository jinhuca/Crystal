using Xunit;
using Crystal.Smbios.Structures;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class AdditionalInformationTests
{
    [Fact]
    public void Decode_SingleEntry_PopulatesFieldsAndValueBytes()
    {
        var payload = new byte[]
        {
            1,          // NumberOfAdditionalInformationEntries
            0x06,       // EntryLength = 6 (2+1+1+1+1 value byte)
            0x04, 0x00, // ReferencedHandle = 0x0004
            0x06,       // ReferencedOffset
            1,          // EntryString #1
            0xFE,       // Value (1 byte)
        };

        var table = MakeTable(MakeStructure(40, 0x0210, payload, new[] { "Processor Family Override" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var a = smbios.AdditionalInformation[0];
        Assert.Equal(1, a.NumberOfAdditionalInformationEntries);
        Assert.Single(a.Entries);

        var entry = a.Entries[0];
        Assert.Equal((ushort)0x0004, entry.ReferencedHandle);
        Assert.Equal((byte)0x06, entry.ReferencedOffset);
        Assert.Equal("Processor Family Override", entry.EntryString);
        Assert.Single(entry.Value);
        Assert.Equal((byte)0xFE, entry.Value[0]);
    }

    [Fact]
    public void Decode_ZeroEntries_EmptyList()
    {
        var table = MakeTable(MakeStructure(40, 0x0211, new byte[] { 0 }));
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Empty(smbios.AdditionalInformation[0].Entries);
    }
}
