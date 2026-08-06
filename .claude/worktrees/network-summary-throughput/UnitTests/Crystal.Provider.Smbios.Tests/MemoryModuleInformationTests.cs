using Xunit;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class MemoryModuleInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        var payload = new byte[0x0C - 4];
        payload[0x00] = 1;    // SocketDesignation string #1
        payload[0x01] = 0x12; // BankConnections
        payload[0x02] = 60;   // CurrentSpeedNs
        payload[0x03] = 0x00; payload[0x04] = 0x01; // CurrentMemoryType = Dimm
        payload[0x05] = 0x8A; // InstalledSizeRaw: double-bank (bit7), n=0x0A -> 1024 MiB
        payload[0x06] = 0x0A; // EnabledSizeRaw: n=0x0A -> 1024 MiB
        payload[0x07] = 0x02; // ErrorStatus = CorrectableErrorsReceived

        var table = MakeTable(MakeStructure(6, 0x0110, payload, new[] { "Bank0/1" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var m = smbios.MemoryModules[0];
        Assert.Equal("Bank0/1", m.SocketDesignation);
        Assert.Equal(0x12, m.BankConnections);
        Assert.Equal(60, m.CurrentSpeedNs);
        Assert.Equal(MemoryModuleTypeFlags.Dimm, m.CurrentMemoryType);
        Assert.True(m.IsDoubleBank);
        Assert.Equal(1024L, m.InstalledSizeMiB);
        Assert.Equal(1024L, m.EnabledSizeMiB);
        Assert.Equal(MemoryModuleErrorStatus.CorrectableErrorsReceived, m.ErrorStatus);
    }

    [Fact]
    public void DecodeSizeMiB_SentinelValues_ReturnNull()
    {
        Assert.Null(T006_MemoryModuleInformation.DecodeSizeMiB(MemoryModuleSizeSentinel.NotDeterminable));
        Assert.Null(T006_MemoryModuleInformation.DecodeSizeMiB(MemoryModuleSizeSentinel.ModuleInstalledNoMemoryEnabled));
        Assert.Null(T006_MemoryModuleInformation.DecodeSizeMiB(MemoryModuleSizeSentinel.NotInstalled));
        Assert.Equal(4096L, T006_MemoryModuleInformation.DecodeSizeMiB(12));
    }
}
