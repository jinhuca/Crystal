using Xunit;
using Crystal.Smbios.Structures;
using Crystal.Smbios.Types;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class MemoryControllerInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFieldsAndSlotHandles()
    {
        var payload = new byte[0x13 - 4];
        payload[0x00] = 0x06; // ErrorDetectingMethod = Ecc64Bit
        payload[0x01] = 0x18; // ErrorCorrectingCapability = SingleBit | DoubleBit
        payload[0x02] = 0x05; // SupportedInterleave = FourWay
        payload[0x03] = 0x04; // CurrentInterleave = TwoWay
        payload[0x04] = 10;   // MaxMemoryModuleSize raw -> 2^10 = 1024 MiB
        payload[0x05] = 0x0C; payload[0x06] = 0x00; // SupportedSpeeds = SeventyNs|SixtyNs
        payload[0x07] = 0x40; payload[0x08] = 0x01; // SupportedMemoryTypes = Ecc|Dimm
        payload[0x09] = 0x03; // SupportedVoltages = FiveVolt|ThreePoint3Volt
        payload[0x0A] = 2;    // AssociatedMemorySlotCount
        payload[0x0B] = 0x10; payload[0x0C] = 0x00; // handle 0x0010
        payload[0x0D] = 0x11; payload[0x0E] = 0x00; // handle 0x0011

        var table = MakeTable(MakeStructure(5, 0x0100, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.MemoryControllers[0];
        Assert.Equal(MemoryControllerErrorDetectingMethod.Ecc64Bit, m.ErrorDetectingMethod);
        Assert.Equal(
            MemoryControllerErrorCorrectingCapability.SingleBitErrorCorrecting | MemoryControllerErrorCorrectingCapability.DoubleBitErrorCorrecting,
            m.ErrorCorrectingCapability);
        Assert.Equal(MemoryControllerInterleaveType.FourWay, m.SupportedInterleave);
        Assert.Equal(MemoryControllerInterleaveType.TwoWay, m.CurrentInterleave);
        Assert.Equal(1024L, m.MaximumMemoryModuleSizeMiB);
        Assert.Equal(MemoryControllerSpeedFlags.SeventyNs | MemoryControllerSpeedFlags.SixtyNs, m.SupportedSpeeds);
        Assert.Equal(MemoryModuleTypeFlags.Ecc | MemoryModuleTypeFlags.Dimm, m.SupportedMemoryTypes);
        Assert.Equal(MemoryModuleVoltageFlags.FiveVolt | MemoryModuleVoltageFlags.ThreePoint3Volt, m.SupportedVoltages);
        Assert.Equal(2, m.AssociatedMemorySlotCount);
        Assert.Equal(2, m.AssociatedMemorySlotHandles.Count);
        Assert.Equal((ushort)0x0010, m.AssociatedMemorySlotHandles[0]);
        Assert.Equal((ushort)0x0011, m.AssociatedMemorySlotHandles[1]);
    }

    [Fact]
    public void Decode_ZeroSlots_EmptyHandleList()
    {
        var payload = new byte[0x0F - 4];
        payload[0x0A] = 0; // AssociatedMemorySlotCount = 0

        var table = MakeTable(MakeStructure(5, 0x0101, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.MemoryControllers[0];
        Assert.Equal(0, m.AssociatedMemorySlotCount);
        Assert.Empty(m.AssociatedMemorySlotHandles);
    }
}
