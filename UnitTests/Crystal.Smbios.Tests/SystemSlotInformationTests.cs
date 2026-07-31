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
        // DSP0134 §7.10 with no peer groups (PeerGroupingCount @0x12 = 0):
        //   0x11 DataBusWidth (base), 0x12 PeerGroupingCount, 0x13 SlotInformation,
        //   0x14 SlotPhysicalWidth, 0x15 SlotPitch WORD, 0x17 SlotHeight.
        // Structure length = 0x18, so payload spans offsets 0x04..0x17 (20 bytes).
        var payload = new byte[0x18 - 4];
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
        payload[0x0D] = 0;                    // DataBusWidth (base) @0x11
        payload[0x0E] = peerGroupingCount;    // PeerGroupingCount @0x12
        payload[0x0F] = 0;                    // SlotInformation @0x13
        payload[0x10] = physicalWidth;        // SlotPhysicalWidth @0x14
        payload[0x11] = (byte)slotPitch;      // SlotPitch WORD @0x15
        payload[0x12] = (byte)(slotPitch >> 8);
        payload[0x13] = (byte)height;         // SlotHeight @0x17
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

    [Fact]
    public void Decode_PeerGroups_ParsedAndTailFieldsFollow()
    {
        // DSP0134 §7.10: PeerGroupingCount @0x12, then n * 5-byte peer groups
        // @0x13, then the v3.4 tail (SlotInformation, PhysicalWidth, Pitch WORD,
        // Height). With 2 peer groups the tail begins at 0x13 + 10 = 0x1D.
        // Structure length = tail + 5 = 0x22 -> payload spans 0x04..0x21 (30 bytes).
        var payload = new byte[0x22 - 4];
        payload[0x00] = 1;                              // SlotDesignation string
        payload[0x01] = (byte)SystemSlotType.PciExpressGen5X16;
        payload[0x02] = (byte)SlotDataBusWidth.X16;
        payload[0x03] = (byte)SlotUsage.InUse;
        payload[0x04] = (byte)SlotLength.LongLength;
        payload[0x0E] = 2;                              // PeerGroupingCount @0x12

        // Peer group 0 @0x13 (payload index 0x0F): seg=0x0001, bus=0x02, devfn=0x18, width=0x0D
        payload[0x0F] = 0x01; payload[0x10] = 0x00; payload[0x11] = 0x02; payload[0x12] = 0x18; payload[0x13] = 0x0D;
        // Peer group 1 @0x18 (payload index 0x14): seg=0x0003, bus=0x04, devfn=0x28, width=0x0B
        payload[0x14] = 0x03; payload[0x15] = 0x00; payload[0x16] = 0x04; payload[0x17] = 0x28; payload[0x18] = 0x0B;

        // Tail @0x1D (payload index 0x19): SlotInformation, PhysicalWidth, Pitch WORD, Height
        payload[0x19] = 0x07;                           // SlotInformation @0x1D
        payload[0x1A] = 8;                              // SlotPhysicalWidth @0x1E
        payload[0x1B] = 55; payload[0x1C] = 0x00;       // SlotPitch @0x1F = 55
        payload[0x1D] = (byte)SlotHeight.FullHeight;    // SlotHeight @0x21

        var smbios = SmbiosTable.FromRawTableData(MakeTable(MakeStructure(9, 0x0037, payload, new[] { "PEER_SLOT" })));
        var slot = smbios.SystemSlots.First();

        Assert.Equal(2, slot.PeerGroupingCount);
        Assert.Equal(2, slot.PeerGroups.Count);
        Assert.Equal((ushort)0x0001, slot.PeerGroups[0].SegmentGroupNumber);
        Assert.Equal((byte)0x02, slot.PeerGroups[0].BusNumber);
        Assert.Equal(3, slot.PeerGroups[0].DeviceNumber);
        Assert.Equal((byte)0x0B, slot.PeerGroups[1].DataBusWidth);

        // Tail fields must be read from after the peer-group array, not a fixed offset.
        Assert.Equal((byte)0x07, slot.SlotInformation);
        Assert.Equal(8, slot.SlotPhysicalWidth);
        Assert.Equal(55, slot.SlotPitch);
        Assert.Equal(SlotHeight.FullHeight, slot.SlotHeight);
    }
}
