using System.Linq;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using Xunit;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class SystemSlotInformationTests
{
    private static byte[] MakeSlotPayload(
        SystemSlotType type = SystemSlotType.PciExpressGen4X16,
        SlotDataBusWidth busWidth = SlotDataBusWidth.X16,
        SlotUsage usage = SlotUsage.InUse,
        SlotLength length = SlotLength.LongLength,
        ushort slotId = 1,
        SlotCharacteristics1 char1 = SlotCharacteristics1.Provides33Volts,
        SlotCharacteristics2 char2 = SlotCharacteristics2.PciHotPlugSupported,
        ushort segmentGroup = 0,
        byte busNumber = 0x01,
        byte deviceFunction = 0x00, // device 0, function 0
        byte physicalWidth = 16,
        ushort slotPitch = 0,
        SlotHeight height = SlotHeight.FullHeight,
        byte peerGroupingCount = 0)
    {
        var payload = new byte[0x16 - 4];
        payload[0x00] = 1; // SlotDesignation string
        payload[0x01] = (byte)type;
        payload[0x02] = (byte)busWidth;
        payload[0x03] = (byte)usage;
        payload[0x04] = (byte)length;
        payload[0x05] = (byte)slotId;
        payload[0x06] = (byte)(slotId >> 8);
        payload[0x07] = (byte)char1;
        payload[0x08] = (byte)char2;
        payload[0x09] = (byte)segmentGroup;
        payload[0x0A] = (byte)(segmentGroup >> 8);
        payload[0x0B] = busNumber;
        payload[0x0C] = deviceFunction;
        payload[0x0D] = physicalWidth;
        payload[0x0E] = (byte)slotPitch;
        payload[0x0F] = (byte)(slotPitch >> 8);
        payload[0x10] = (byte)height;
        payload[0x11] = peerGroupingCount;
        return payload;
    }

    [Fact]
    public void Decode_PopulatesAllBaseFields()
    {
        var payload = MakeSlotPayload(
            type: SystemSlotType.PciExpressGen4X16,
            busWidth: SlotDataBusWidth.X16,
            usage: SlotUsage.InUse,
            slotId: 3);
        var table  = MakeTable(MakeStructure(9, 0x0030, payload, new[] { "PCIEX16_1" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var slot = smbios.SystemSlots.FirstOrDefault();
        Assert.NotNull(slot);
        Assert.Equal("PCIEX16_1", slot!.SlotDesignation);
        Assert.Equal(SystemSlotType.PciExpressGen4X16, slot.SlotType);
        Assert.Equal(SlotDataBusWidth.X16, slot.DataBusWidth);
        Assert.Equal(SlotUsage.InUse, slot.CurrentUsage);
        Assert.Equal(3, slot.SlotId);
        Assert.True(slot.IsInUse);
    }

    [Fact]
    public void Decode_EmptySlot_IsInUseFalse()
    {
        var payload = MakeSlotPayload(usage: SlotUsage.Available);
        var table   = MakeTable(MakeStructure(9, 0x0031, payload, new[] { "M2_1" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var slot = smbios.SystemSlots.First();
        Assert.False(slot.IsInUse);
    }

    [Fact]
    public void Decode_Characteristics_DecodedAsFlags()
    {
        var payload = MakeSlotPayload(
            char1: SlotCharacteristics1.Provides33Volts | SlotCharacteristics1.SharedSlot,
            char2: SlotCharacteristics2.PciHotPlugSupported | SlotCharacteristics2.BifurcationSupported);
        var table  = MakeTable(MakeStructure(9, 0x0032, payload, new[] { "SLOT1" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var slot = smbios.SystemSlots.First();
        Assert.True(slot.Characteristics1.HasFlag(SlotCharacteristics1.Provides33Volts));
        Assert.True(slot.Characteristics1.HasFlag(SlotCharacteristics1.SharedSlot));
        Assert.True(slot.Characteristics2.HasFlag(SlotCharacteristics2.PciHotPlugSupported));
        Assert.True(slot.Characteristics2.HasFlag(SlotCharacteristics2.BifurcationSupported));
    }

    [Fact]
    public void Decode_DeviceFunctionNumber_SplitCorrectly()
    {
        // Device 5, Function 2 → (5 << 3) | 2 = 0x2A
        byte packed = (5 << 3) | 2;
        var payload = MakeSlotPayload(deviceFunction: packed);
        var table   = MakeTable(MakeStructure(9, 0x0033, payload, new[] { "SLOT2" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var slot = smbios.SystemSlots.First();
        Assert.Equal(5, slot.DeviceNumber);
        Assert.Equal(2, slot.FunctionNumber);
    }

    [Fact]
    public void Decode_BusNumber_DecodedCorrectly()
    {
        var payload = MakeSlotPayload(busNumber: 0x04);
        var table   = MakeTable(MakeStructure(9, 0x0034, payload, new[] { "SLOT3" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(0x04, smbios.SystemSlots.First().BusNumber);
    }

    [Fact]
    public void Decode_LegacyShortStructure_NoV32Fields_UsesDefaults()
    {
        // A v2.6-era structure — only through DeviceFunctionNumber (0x11 bytes total).
        var payload = new byte[0x11 - 4];
        payload[0x00] = 1;
        payload[0x01] = (byte)SystemSlotType.Pci;
        payload[0x02] = (byte)SlotDataBusWidth.Bit32;
        payload[0x03] = (byte)SlotUsage.Available;
        payload[0x04] = (byte)SlotLength.LongLength;

        var table  = MakeTable(MakeStructure(9, 0x0035, payload, new[] { "PCI1" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var slot = smbios.SystemSlots.First();
        Assert.Equal(0xFF, slot.SlotPhysicalWidth);          // default sentinel
        Assert.Equal(SlotHeight.NotApplicable, slot.SlotHeight);
        Assert.Equal(0, slot.PeerGroupingCount);
    }

    [Fact]
    public void PopulatedSlots_FiltersToInUseOnly()
    {
        var s1 = MakeStructure(9, 0x0040, MakeSlotPayload(usage: SlotUsage.InUse), new[] { "SLOT_A" });
        var s2 = MakeStructure(9, 0x0041, MakeSlotPayload(usage: SlotUsage.Available), new[] { "SLOT_B" });
        var s3 = MakeStructure(9, 0x0042, MakeSlotPayload(usage: SlotUsage.InUse), new[] { "SLOT_C" });

        var table  = MakeTable(s1, s2, s3);
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Equal(3, smbios.SystemSlots.Count);
        Assert.Equal(2, smbios.PopulatedSlots.Count());
    }

    [Fact]
    public void Decode_SlotPitch_And_PhysicalWidth_V32Fields()
    {
        var payload = MakeSlotPayload(physicalWidth: 4, slotPitch: 55); // x4 physical, 5.5mm pitch
        var table   = MakeTable(MakeStructure(9, 0x0036, payload, new[] { "M2_SLOT" }));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var slot = smbios.SystemSlots.First();
        Assert.Equal(4, slot.SlotPhysicalWidth);
        Assert.Equal(55, slot.SlotPitch);
    }
}
