using Crystal.Provider.Smbios.Structures;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class SystemConfigurationOptionsTests
{
    [Fact]
    public void Decode_PopulatesOptionsAndCount()
    {
        var payload = new byte[] { 2 }; // two option strings reported

        var table = MakeTable(MakeStructure(12, 0x0130, payload, new[] { "JP1: Clear CMOS", "JP2: Recovery Mode" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var o = smbios.SystemConfigurationOptions[0];
        Assert.Equal(2, o.StringCount);
        Assert.Equal(2, o.Options.Count);
        Assert.Equal("JP1: Clear CMOS", o.Options[0]);
        Assert.Equal("JP2: Recovery Mode", o.Options[1]);
    }

    [Fact]
    public void Decode_ZeroOptions_EmptyList()
    {
        var payload = new byte[] { 0 };
        var table = MakeTable(MakeStructure(12, 0x0131, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var o = smbios.SystemConfigurationOptions[0];
        Assert.Equal(0, o.StringCount);
        Assert.Empty(o.Options);
    }
}
