using System.Linq;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using Xunit;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class OnboardDeviceExtendedInformationTests
{
    private static byte[] MakePayload(
        OnboardDeviceType deviceType = OnboardDeviceType.Ethernet,
        bool enabled = true,
        byte instance = 1,
        ushort segmentGroup = 0,
        byte busNumber = 0x02,
        byte deviceFunction = 0x00)
    {
        var payload = new byte[0x0B - 4];
        payload[0x00] = 1; // ReferenceDesignation string
        byte typeByte = (byte)((byte)deviceType & 0x7F);
        if (enabled) typeByte |= 0x80;
        payload[0x01] = typeByte;
        payload[0x02] = instance;
        payload[0x03] = (byte)segmentGroup;
        payload[0x04] = (byte)(segmentGroup >> 8);
        payload[0x05] = busNumber;
        payload[0x06] = deviceFunction;
        return payload;
    }

    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = MakePayload(deviceType: OnboardDeviceType.Ethernet, enabled: true, instance: 1, busNumber: 0x03);
        var table   = MakeTable(MakeStructure(41, 0x0050, payload, new[] { "Onboard LAN" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var device = smbios.OnboardDevices.FirstOrDefault();
        Assert.NotNull(device);
        Assert.Equal("Onboard LAN", device!.ReferenceDesignation);
        Assert.Equal(OnboardDeviceType.Ethernet, device.DeviceType);
        Assert.True(device.IsEnabled);
        Assert.Equal(1, device.DeviceTypeInstance);
        Assert.Equal(0x03, device.BusNumber);
    }

    [Fact]
    public void Decode_DisabledDevice_IsEnabledFalse()
    {
        var payload = MakePayload(deviceType: OnboardDeviceType.Sound, enabled: false);
        var table   = MakeTable(MakeStructure(41, 0x0051, payload, new[] { "Onboard Audio" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var device = smbios.OnboardDevices.First();
        Assert.False(device.IsEnabled);
        Assert.Equal(OnboardDeviceType.Sound, device.DeviceType);
    }

    [Fact]
    public void Decode_DeviceTypeBit_DoesNotLeakIntoEnabledFlag()
    {
        // DeviceType = 0x7F (max 7-bit value) with enabled bit also set (0xFF raw).
        // Ensures the mask/flag split is bit-exact and doesn't overflow into each other.
        var payload = new byte[0x0B - 4];
        payload[0x00] = 1;
        payload[0x01] = 0xFF; // all bits set: type=0x7F (undefined but shouldn't crash), enabled=true
        var table  = MakeTable(MakeStructure(41, 0x0052, payload, new[] { "Weird Device" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var device = smbios.OnboardDevices.First();
        Assert.True(device.IsEnabled);
        Assert.Equal((OnboardDeviceType)0x7F, device.DeviceType);
    }

    [Fact]
    public void Decode_DeviceFunctionNumber_SplitCorrectly()
    {
        byte packed = (2 << 3) | 1; // device 2, function 1
        var payload = MakePayload(deviceFunction: packed);
        var table   = MakeTable(MakeStructure(41, 0x0053, payload, new[] { "Onboard NVMe" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var device = smbios.OnboardDevices.First();
        Assert.Equal(2, device.DeviceNumber);
        Assert.Equal(1, device.FunctionNumber);
    }

    [Fact]
    public void Decode_LegacyShortStructure_MissingPciFields_UsesDefaults()
    {
        // Minimal structure with only ReferenceDesignation, DeviceType, DeviceTypeInstance —
        // no PCI addressing fields present (length stops right after offset 0x06).
        var payload = new byte[0x07 - 4];
        payload[0x00] = 1;
        payload[0x01] = (byte)OnboardDeviceType.Video | 0x80;
        payload[0x02] = 1;

        var table  = MakeTable(MakeStructure(41, 0x0054, payload, new[] { "Onboard VGA" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var device = smbios.OnboardDevices.First();
        Assert.Equal(0, device.SegmentGroupNumber);
        Assert.Equal(0, device.BusNumber);
        Assert.Equal(0, device.DeviceFunctionNumber);
    }

    [Fact]
    public void EnabledOnboardDevices_FiltersToEnabledOnly()
    {
        var d1 = MakeStructure(41, 0x0060, MakePayload(enabled: true),  new[] { "LAN" });
        var d2 = MakeStructure(41, 0x0061, MakePayload(enabled: false), new[] { "Bluetooth" });
        var d3 = MakeStructure(41, 0x0062, MakePayload(enabled: true),  new[] { "Audio" });

        var table  = MakeTable(d1, d2, d3);
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Equal(3, smbios.OnboardDevices.Count);
        Assert.Equal(2, smbios.EnabledOnboardDevices.Count());
    }

    [Fact]
    public void Decode_SegmentGroupNumber_NonZero_DecodedCorrectly()
    {
        var payload = MakePayload(segmentGroup: 0x0002);
        var table   = MakeTable(MakeStructure(41, 0x0055, payload, new[] { "Onboard SAS" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(0x0002, smbios.OnboardDevices.First().SegmentGroupNumber);
    }
}
