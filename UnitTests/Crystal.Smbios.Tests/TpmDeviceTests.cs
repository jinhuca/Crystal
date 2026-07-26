using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class TpmDeviceTests
{
    [Fact]
    public void Decode_PopulatesVendorVersionAndCharacteristics()
    {
        var payload = new byte[0x1F - 4];
        payload[0x00] = (byte)'I'; payload[0x01] = (byte)'F'; payload[0x02] = (byte)'X'; payload[0x03] = 0x00; // VendorID
        payload[0x04] = 2; // MajorSpecVersion
        payload[0x05] = 0; // MinorSpecVersion
        payload[0x06] = 0x03; payload[0x07] = 0x02; payload[0x08] = 0x01; payload[0x09] = 0x00; // FirmwareVersion1 = 0x00010203
        // FirmwareVersion2 (0x0E-0x11) left zero
        payload[0x0E] = 1; // Description string #1
        payload[0x0F] = 0x08; // Characteristics low byte: FamilyConfigurableViaFirmwareUpdate (bit3)
        // remaining Characteristics bytes and OemDefined left zero

        var table = MakeTable(MakeStructure(43, 0x0230, payload, new[] { "TPM 2.0" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var t = smbios.Tpm;
        Assert.NotNull(t);
        Assert.Equal("IFX", t!.VendorId);
        Assert.Equal((byte)2, t.MajorSpecVersion);
        Assert.Equal((byte)0, t.MinorSpecVersion);
        Assert.Equal(0x00010203u, t.FirmwareVersion1);
        Assert.Equal("TPM 2.0", t.Description);
        Assert.True(t.Characteristics.HasFlag(TpmDeviceCharacteristics.FamilyConfigurableViaFirmwareUpdate));
        Assert.Equal(0u, t.OemDefined);
    }
}
