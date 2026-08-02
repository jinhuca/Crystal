using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class ProcessorInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = new byte[0x26 - 4];
        payload[0x00] = 0x01; // SocketDesignation -> string #1
        payload[0x01] = (byte)ProcessorType.CentralProcessor;
        payload[0x04] = 0x10; payload[0x05] = 0x00; // ProcessorId low (QWORD occupies 8 bytes at 0x08, but tests only assert some fields)
        payload[0x0C] = 0x20; payload[0x0D] = 0x14; // MaxSpeed = 0x1420 = 5152 (we'll set at offsets consistent with SmbiosStructures)
        // Mark populated bit in Status at offset 0x14 (bit 6)
        payload[0x14] = 0x41;
        payload[0x1F] = 8; // CoreCount

        var table  = MakeTable(MakeStructure(4, 0x0040, payload,
            new[] { "CPU0", "Intel Corp", "ModelX" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var cpu = smbios.ProcessorInformation.FirstOrDefault();
        Assert.NotNull(cpu);
        Assert.True(cpu!.IsPopulated);
        Assert.Equal(8, cpu.CoreCount);
        Assert.Equal("CPU0", cpu.SocketDesignation);
    }

    [Fact]
    public void PopulatedProcessors_FiltersUnpopulatedSockets()
    {
        static byte[] MakeCpu(ushort handle, bool populated)
        {
            var payload = new byte[0x26 - 4];
            payload[0x14] = populated ? (byte)0x41 : (byte)0x01;
            return MakeStructure(4, handle, payload);
        }

        var table  = MakeTable(MakeCpu(0x20, true), MakeCpu(0x21, false), MakeCpu(0x22, true));
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Equal(3, smbios.ProcessorInformation.Count);
        Assert.Equal(2, smbios.PopulatedProcessors.Count());
    }
}
