using Xunit;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class FirmwareInventoryInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFieldsAndAssociatedHandles()
    {
        var payload = new byte[]
        {
            1,          // FirmwareComponentName string #1 "BIOS"
            2,          // FirmwareVersion string #2 "1.2.3"
            0x01,       // FirmwareVersionFormat = MajorMinor
            3,          // FirmwareId string #3
            0x00,       // FirmwareIdFormat = FreeForm
            4,          // ReleaseDate string #4
            5,          // Manufacturer string #5
            0,          // LowestSupportedVersion string #0 = none
            0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, // ImageSizeBytes = 2097152
            0x01, 0x00, // Characteristics = Updatable
            0x04,       // State = Enabled
            1,          // AssociatedComponentCount
            0x50, 0x00, // handle 0x0050
        };

        var strings = new[] { "BIOS", "1.2.3", "{11111111-2222-3333-4444-555555555555}", "07/01/2026", "Acme Corp" };
        var table = MakeTable(MakeStructure(45, 0x0250, payload, strings));
        var smbios = SmbiosTable.FromRawTableData(table);

        var f = smbios.FirmwareInventory[0];
        Assert.Equal("BIOS", f.FirmwareComponentName);
        Assert.Equal("1.2.3", f.FirmwareVersion);
        Assert.Equal(FirmwareVersionFormat.MajorMinor, f.FirmwareVersionFormat);
        Assert.Equal(FirmwareIdFormat.FreeForm, f.FirmwareIdFormat);
        Assert.Equal("07/01/2026", f.ReleaseDate);
        Assert.Equal("Acme Corp", f.Manufacturer);
        Assert.Null(f.LowestSupportedVersion);
        Assert.Equal(2097152ul, f.ImageSizeBytes);
        Assert.Equal(FirmwareCharacteristics.Updatable, f.Characteristics);
        Assert.Equal(FirmwareInventoryState.Enabled, f.State);
        Assert.Single(f.AssociatedComponentHandles);
        Assert.Equal((ushort)0x0050, f.AssociatedComponentHandles[0]);
    }

    [Fact]
    public void Decode_NoAssociatedComponents_EmptyList()
    {
        var payload = new byte[]
        {
            0, 0, 0x00, 0, 0x00, 0, 0, 0,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00,
            0x02, // State = Unknown
            0,    // AssociatedComponentCount = 0
        };

        var table = MakeTable(MakeStructure(45, 0x0251, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Empty(smbios.FirmwareInventory[0].AssociatedComponentHandles);
    }
}
