using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using System.Linq;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class BaseboardInformationTests
{
    [Fact]
    public void Decode_PopulatesAllFields()
    {
        // DSP0134 §7.3: Manufacturer@0x04, Product@0x05, Version@0x06,
        // SerialNumber@0x07, AssetTag@0x08, FeatureFlags@0x09,
        // LocationInChassis@0x0A, ChassisHandle WORD@0x0B, BoardType@0x0D,
        // ContainedObjectHandleCount@0x0E, handles@0x0F+.
        // No contained handles -> length 0x0F. Payload index = offset - 4.
        var payload = new byte[0x0F - 4];
        payload[0x00] = 1; // Manufacturer
        payload[0x01] = 2; // Product
        payload[0x02] = 3; // Version
        payload[0x03] = 4; // SerialNumber
        payload[0x04] = 5; // AssetTag
        payload[0x05] = (byte)(BaseboardFeatureFlags.HostingBoard | BaseboardFeatureFlags.Replaceable); // FeatureFlags @0x09
        payload[0x06] = 6; // LocationInChassis @0x0A
        payload[0x07] = 0x34; payload[0x08] = 0x12; // ChassisHandle @0x0B = 0x1234
        payload[0x09] = (byte)BaseboardType.Motherboard; // BoardType @0x0D
        payload[0x0A] = 0x00; // ContainedObjectHandleCount @0x0E = 0

        var table = MakeTable(MakeStructure(2, 0x0002, payload,
            new[] { "ASUSTeK", "ROG STRIX Z790", "Rev 1.0", "SN-BOARD-01", "Default string", "Base of Chassis" }));
        var board = SmbiosTable.FromRawTableData(table).Baseboard;

        Assert.NotNull(board);
        Assert.Equal("ASUSTeK", board!.Manufacturer);
        Assert.Equal("ROG STRIX Z790", board.Product);
        Assert.Equal("Rev 1.0", board.Version);
        Assert.Equal("SN-BOARD-01", board.SerialNumber);
        Assert.Equal("Default string", board.AssetTag);
        Assert.Equal("Base of Chassis", board.LocationInChassis);
        Assert.Equal((ushort)0x1234, board.ChassisHandle);
        Assert.Equal(BaseboardType.Motherboard, board.BoardType);
        Assert.True(board.IsHostingBoard);
        Assert.False(board.IsHotSwappable);
        Assert.True(board.FeatureFlags.HasFlag(BaseboardFeatureFlags.Replaceable));
        Assert.Empty(board.ContainedObjectHandles);
    }

    [Fact]
    public void Decode_ContainedObjectHandles_ParsedFromVariableArray()
    {
        // 2 contained handles -> length 0x0F + 2*2 = 0x13.
        var payload = new byte[0x13 - 4];
        payload[0x00] = 1; // Manufacturer
        payload[0x05] = (byte)BaseboardFeatureFlags.HostingBoard; // FeatureFlags @0x09
        payload[0x09] = (byte)BaseboardType.ProcessorModule; // BoardType @0x0D
        payload[0x0A] = 0x02; // ContainedObjectHandleCount @0x0E = 2
        payload[0x0B] = 0x10; payload[0x0C] = 0x00; // handle[0] @0x0F = 0x0010
        payload[0x0D] = 0x20; payload[0x0E] = 0x00; // handle[1] @0x11 = 0x0020

        var table = MakeTable(MakeStructure(2, 0x0003, payload, new[] { "Vendor" }));
        var board = SmbiosTable.FromRawTableData(table).Baseboard;

        Assert.NotNull(board);
        Assert.Equal(BaseboardType.ProcessorModule, board!.BoardType);
        Assert.Equal(2, board.ContainedObjectHandles.Length);
        Assert.Equal((ushort)0x0010, board.ContainedObjectHandles[0]);
        Assert.Equal((ushort)0x0020, board.ContainedObjectHandles[1]);
    }

    [Fact]
    public void Decode_LegacyShortStructure_UsesDefaults()
    {
        // v2.0-era board: only through SerialNumber (length 0x08).
        var payload = new byte[0x08 - 4];
        payload[0x00] = 1; // Manufacturer
        payload[0x01] = 2; // Product

        var table = MakeTable(MakeStructure(2, 0x0004, payload, new[] { "OldVendor", "OldBoard" }));
        var board = SmbiosTable.FromRawTableData(table).Baseboard;

        Assert.NotNull(board);
        Assert.Equal("OldVendor", board!.Manufacturer);
        Assert.Equal("OldBoard", board.Product);
        Assert.Null(board.AssetTag);
        Assert.Equal(BaseboardFeatureFlags.None, board.FeatureFlags);
        Assert.Equal((ushort)0, board.ChassisHandle);
        Assert.Equal(BaseboardType.Unknown, board.BoardType);
        Assert.Empty(board.ContainedObjectHandles);
    }
}
