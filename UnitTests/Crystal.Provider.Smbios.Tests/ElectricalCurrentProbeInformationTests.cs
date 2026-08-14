using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class ElectricalCurrentProbeInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFieldsIncludingNominal()
    {
        var payload = new byte[]
        {
            1,          // Description string #1
            0x63,       // Location=Processor(3) | Status=OK(3)<<5
            0xDC, 0x05, // MaximumValue = 1500 mA
            0x00, 0x00, // MinimumValue = 0 mA
            0x64, 0x00, // Resolution = 100
            0x32, 0x00, // Tolerance = 50
            0x48, 0x26, // Accuracy = 9800
            0x00, 0x00, 0x00, 0x00, // OEMDefined = 0
            0xB0, 0x04, // NominalValue = 1200 mA
        };

        var table = MakeTable(MakeStructure(29, 0x0180, payload, new[] { "CPU Current" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var e = smbios.ElectricalCurrentProbes[0];
        Assert.Equal("CPU Current", e.Description);
        Assert.Equal(ElectricalCurrentProbeLocation.Processor, e.Location);
        Assert.Equal(ElectricalCurrentProbeStatus.OK, e.Status);
        Assert.Equal((ushort)1500, e.MaximumValueMilliamps);
        Assert.Equal((ushort)0, e.MinimumValueMilliamps);
        Assert.True(e.IsMinValueIdentifiable);
        Assert.Equal((ushort)100, e.ResolutionMicroamps);
        Assert.Equal((ushort)50, e.ToleranceMilliamps);
        Assert.Equal(9800u, e.Accuracy);
        Assert.Equal((ushort)1200, e.NominalValueMilliamps);
        Assert.True(e.IsNominalValueIdentifiable);
    }

    [Fact]
    public void Decode_ShortStructureWithoutNominalValue_DefaultsToUnknown()
    {
        var payload = new byte[]
        {
            0,
            0x22,       // Location=Unknown(2) | Status=Other(1)<<5=0x20 -> 0x22
            0x00, 0x00,
            0x00, 0x00,
            0x00, 0x00,
            0x00, 0x00,
            0x00, 0x00,
        };

        var table = MakeTable(MakeStructure(29, 0x0181, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var e = smbios.ElectricalCurrentProbes[0];
        Assert.Equal(0u, e.OEMDefined);
        Assert.Equal((ushort)0x8000, e.NominalValueMilliamps);
        Assert.False(e.IsNominalValueIdentifiable);
    }
}
