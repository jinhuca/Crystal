using Xunit;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class GroupAssociationsTests
{
    [Fact]
    public void Decode_PopulatesGroupNameAndItems()
    {
        var payload = new byte[]
        {
            1,             // GroupName string #1
            0, 0x01, 0x00, // ItemType = BiosInformation (0), handle 0x0001
            4, 0x04, 0x00, // ItemType = ProcessorInformation (4), handle 0x0004
        };

        var table = MakeTable(MakeStructure(14, 0x0140, payload, new[] { "RAID Array 1" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var g = smbios.GroupAssociations[0];
        Assert.Equal("RAID Array 1", g.GroupName);
        Assert.Equal(2, g.Items.Count);
        Assert.Equal(SmbiosStructureType.BiosInformation, g.Items[0].ItemType);
        Assert.Equal((ushort)0x0001, g.Items[0].ItemHandle);
        Assert.Equal(SmbiosStructureType.ProcessorInformation, g.Items[1].ItemType);
        Assert.Equal((ushort)0x0004, g.Items[1].ItemHandle);
    }

    [Fact]
    public void Decode_NoItems_EmptyList()
    {
        var payload = new byte[] { 0 }; // GroupName string #0 = none
        var table = MakeTable(MakeStructure(14, 0x0141, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var g = smbios.GroupAssociations[0];
        Assert.Null(g.GroupName);
        Assert.Empty(g.Items);
    }
}
