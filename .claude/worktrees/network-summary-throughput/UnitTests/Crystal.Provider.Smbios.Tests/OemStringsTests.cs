using Crystal.Provider.Smbios.Structures;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class OemStringsTests
{
    [Fact]
    public void Decode_PopulatesStringsAndCount()
    {
        var payload = new byte[0x05 - 4];
        payload[0x00] = 2; // two strings reported

        var table  = MakeTable(MakeStructure(11, 0x0080, payload, new[] { "One", "Two" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var o = smbios.OemStrings.FirstOrDefault();
        Assert.NotNull(o);
        Assert.Equal(2, o!.NumberOfStrings);
        Assert.Equal(2, o.Strings.Count);
        Assert.Equal("One", o.Strings[0]);
        Assert.Equal("Two", o.Strings[1]);
    }

    [Fact]
    public void Decode_ZeroStrings_EmptyList()
    {
        var payload = new byte[0x05 - 4];
        payload[0x00] = 0;

        var table  = MakeTable(MakeStructure(11, 0x0081, payload, new string[0]));
        var smbios = SmbiosTable.FromRawTableData(table);

        var o = smbios.OemStrings.First();
        Assert.Equal(0, o.NumberOfStrings);
        Assert.Empty(o.Strings);
    }
}
