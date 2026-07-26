using Xunit;
using Crystal.Smbios.Structures;
using static Crystal.Smbios.Tests.TestHelpers;

namespace Crystal.Smbios.Tests;

public class OutOfBandRemoteAccessTests
{
    [Fact]
    public void Decode_BothConnectionsEnabled()
    {
        var payload = new byte[] { 1, 0x03 };
        var table = MakeTable(MakeStructure(30, 0x0190, payload, new[] { "Acme Remote Mgmt" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var r = smbios.OutOfBandRemoteAccess[0];
        Assert.Equal("Acme Remote Mgmt", r.ManufacturerName);
        Assert.True(r.InboundConnectionEnabled);
        Assert.True(r.OutboundConnectionEnabled);
    }

    [Fact]
    public void Decode_NoConnectionsEnabled()
    {
        var payload = new byte[] { 0, 0x00 };
        var table = MakeTable(MakeStructure(30, 0x0191, payload));
        var smbios = SmbiosTable.FromRawTableData(table);

        var r = smbios.OutOfBandRemoteAccess[0];
        Assert.False(r.InboundConnectionEnabled);
        Assert.False(r.OutboundConnectionEnabled);
    }
}
