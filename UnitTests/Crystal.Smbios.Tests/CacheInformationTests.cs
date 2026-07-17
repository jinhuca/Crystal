using System.Linq;
using static Crystal.Smbios.Tests.TestHelpers;
using Xunit;
using Crystal.Smbios.Types;
using Crystal.Smbios.Structures;

namespace Crystal.Smbios.Tests;

public class CacheInformationTests
{
    // ── CacheConfiguration bitfield ────────────────────────────────────────────

    [Fact]
    public void Configuration_DecodesLevel_L2()
    {
        // Level bits (0-2) = 001 → Level 2 (0-based + 1)
        var config = new CacheConfiguration(0b0000_0000_0000_0001);
        Assert.Equal(2, config.Level);
    }

    [Fact]
    public void Configuration_DecodesLevel_L1()
    {
        var config = new CacheConfiguration(0b0000_0000_0000_0000);
        Assert.Equal(1, config.Level);
    }

    [Fact]
    public void Configuration_DecodesLocation_Internal()
    {
        var config = new CacheConfiguration(0b0000_0000_0000_0000); // bits 5-6 = 00
        Assert.Equal(CacheLocation.Internal, config.Location);
    }

    [Fact]
    public void Configuration_DecodesLocation_External()
    {
        var config = new CacheConfiguration(0b0000_0000_0010_0000); // bits 5-6 = 01
        Assert.Equal(CacheLocation.External, config.Location);
    }

    [Fact]
    public void Configuration_DecodesEnabledAtBoot()
    {
        var config = new CacheConfiguration(1 << 7);
        Assert.True(config.EnabledAtBoot);
    }

    [Fact]
    public void Configuration_DecodesDisabledAtBoot()
    {
        var config = new CacheConfiguration(0);
        Assert.False(config.EnabledAtBoot);
    }

    [Fact]
    public void Configuration_DecodesSocketed()
    {
        var config = new CacheConfiguration(1 << 3);
        Assert.True(config.Socketed);
    }

    [Fact]
    public void Configuration_DecodesOperationalMode_WriteBack()
    {
        var config = new CacheConfiguration(0b0000_0001_0000_0000); // bits 8-9 = 01
        Assert.Equal(CacheOperationalMode.WriteBack, config.OperationalMode);
    }

    // ── CacheSizeDecoder ───────────────────────────────────────────────────────

    [Theory]
    [InlineData((ushort)0x0400, 0u, 1024L)]        // legacy, 1K granularity: 1024 KiB
    [InlineData((ushort)0x8010, 0u, 1024L)]        // legacy, 64K granularity: 16 × 64 = 1024 KiB
    public void CacheSizeDecoder_LegacyField_DecodesCorrectly(ushort legacy, uint extended, long expectedKiB)
    {
        Assert.Equal(expectedKiB, CacheSizeDecoder.DecodeKiB(legacy, extended));
    }

    [Fact]
    public void CacheSizeDecoder_ExtendedField_UsedWhenLegacyIsSentinel()
    {
        // extended: bit31 clear (1K granularity), value = 2048 → 2048 KiB
        uint extended = 2048;
        Assert.Equal(2048L, CacheSizeDecoder.DecodeKiB(0xFFFF, extended));
    }

    [Fact]
    public void CacheSizeDecoder_ExtendedField_64KGranularity()
    {
        // bit31 set, value = 32 → 32 × 64 = 2048 KiB
        uint extended = 0x80000000 | 32;
        Assert.Equal(2048L, CacheSizeDecoder.DecodeKiB(0xFFFF, extended));
    }

    [Fact]
    public void CacheSizeDecoder_LegacyZero_ReturnsZero()
    {
        Assert.Equal(0L, CacheSizeDecoder.DecodeKiB(0, 0));
    }

    // ── CacheInformation.Decode ────────────────────────────────────────────────

    private static byte[] MakeCachePayload(
        ushort configuration = 0,
        ushort maxSizeLegacy = 0x0100,     // 256 KiB, 1K granularity
        ushort installedSizeLegacy = 0x0100,
        ushort supportedSram = (ushort)CacheSramType.Synchronous,
        ushort currentSram = (ushort)CacheSramType.Synchronous,
        byte speedNs = 0,
        byte errorCorrection = (byte)CacheErrorCorrectionType.SingleBitEcc,
        byte cacheType = (byte)SystemCacheType.Unified,
        byte associativity = (byte)CacheAssociativity.EightWay)
    {
        var payload = new byte[0x13 - 4];
        payload[0x00] = 1; // SocketDesignation string
        payload[0x01] = (byte)configuration;
        payload[0x02] = (byte)(configuration >> 8);
        payload[0x03] = (byte)maxSizeLegacy;
        payload[0x04] = (byte)(maxSizeLegacy >> 8);
        payload[0x05] = (byte)installedSizeLegacy;
        payload[0x06] = (byte)(installedSizeLegacy >> 8);
        payload[0x07] = (byte)supportedSram;
        payload[0x08] = (byte)(supportedSram >> 8);
        payload[0x09] = (byte)currentSram;
        payload[0x0A] = (byte)(currentSram >> 8);
        payload[0x0B] = speedNs;
        payload[0x0C] = errorCorrection;
        payload[0x0D] = cacheType;
        payload[0x0E] = associativity;
        return payload;
    }

    [Fact]
    public void CacheInformation_Decode_PopulatesAllFields()
    {
        var payload = MakeCachePayload(
            configuration: 0b0000_0000_1010_0001, // L2, external, enabled
            maxSizeLegacy: 0x0100,
            installedSizeLegacy: 0x0080);
        var table  = MakeTable(MakeStructure(7, 0x0007, payload, new[] { "L2-Cache" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var cache = smbios.CacheInformation.FirstOrDefault();
        Assert.NotNull(cache);
        Assert.Equal("L2-Cache", cache!.SocketDesignation);
        Assert.Equal(2, cache.Configuration.Level);
        Assert.Equal(256L, cache.MaxSizeKiB);
        Assert.Equal(128L, cache.InstalledSizeKiB);
        Assert.Equal(CacheSramType.Synchronous, cache.SupportedSramType);
        Assert.Equal(CacheErrorCorrectionType.SingleBitEcc, cache.ErrorCorrectionType);
        Assert.Equal(SystemCacheType.Unified, cache.SystemCacheType);
        Assert.Equal(CacheAssociativity.EightWay, cache.Associativity);
        Assert.Equal(0x0007, cache.Handle);
    }

    [Fact]
    public void CacheInformation_UsesExtendedSize_WhenLegacyIsSentinel()
    {
        var payload = new byte[0x1B - 4];
        payload[0x00] = 1;
        payload[0x01] = 0x00; payload[0x02] = 0x00; // configuration
        payload[0x03] = 0xFF; payload[0x04] = 0xFF; // MaxSize legacy = 0xFFFF sentinel
        payload[0x05] = 0xFF; payload[0x06] = 0xFF; // InstalledSize legacy = 0xFFFF sentinel
        // Extended MaxSize at offset 0x13 (0x0F in payload): 4096 KiB
        var maxExtended = System.BitConverter.GetBytes(4096u);
        maxExtended.CopyTo(payload, 0x0F);
        // Extended InstalledSize at offset 0x17 (0x13 in payload): 2048 KiB
        var instExtended = System.BitConverter.GetBytes(2048u);
        instExtended.CopyTo(payload, 0x13);

        var table  = MakeTable(MakeStructure(7, 0x0008, payload, new[] { "L3-Cache" }));
        var smbios = SmbiosTable.FromRawTableData(table);

        var cache = smbios.CacheInformation.First();
        Assert.Equal(4096L, cache.MaxSizeKiB);
        Assert.Equal(2048L, cache.InstalledSizeKiB);
    }

    // ── SmbiosTable.GetCachesFor (Type 4 ↔ Type 7 join) ───────────────────────

    private static byte[] MakeCpuWithCacheHandles(
        ushort handle, ushort l1, ushort l2, ushort l3)
    {
        var payload = new byte[0x20 - 4];
        payload[0x14] = 0x41; // populated
        payload[0x16] = (byte)l1; payload[0x17] = (byte)(l1 >> 8);
        payload[0x18] = (byte)l2; payload[0x19] = (byte)(l2 >> 8);
        payload[0x1A] = (byte)l3; payload[0x1B] = (byte)(l3 >> 8);
        return MakeStructure(4, handle, payload);
    }

    [Fact]
    public void GetCachesFor_ResolvesL1L2L3_InOrder()
    {
        var cpu = MakeCpuWithCacheHandles(0x0004, 0x0007, 0x0008, 0x0009);
        var l1  = MakeStructure(7, 0x0007, MakeCachePayload(configuration: 0), new[] { "L1" });
        var l2  = MakeStructure(7, 0x0008, MakeCachePayload(configuration: 1), new[] { "L2" });
        var l3  = MakeStructure(7, 0x0009, MakeCachePayload(configuration: 2), new[] { "L3" });

        var table  = MakeTable(cpu, l1, l2, l3);
        var smbios = SmbiosTable.FromRawTableData(table);

        var processor = smbios.ProcessorInformation.First();
        var caches    = smbios.GetCachesFor(processor);

        Assert.Equal(3, caches.Count);
        Assert.Equal(1, caches[0].Configuration.Level);
        Assert.Equal(2, caches[1].Configuration.Level);
        Assert.Equal(3, caches[2].Configuration.Level);
    }

    [Fact]
    public void GetCachesFor_MissingHandle_ReturnsOnlyPresentLevels()
    {
        // L3CacheHandle = 0xFFFF → no L3 cache
        var cpu = MakeCpuWithCacheHandles(0x0004, 0x0007, 0x0008, 0xFFFF);
        var l1  = MakeStructure(7, 0x0007, MakeCachePayload(configuration: 0), new[] { "L1" });
        var l2  = MakeStructure(7, 0x0008, MakeCachePayload(configuration: 1), new[] { "L2" });

        var table  = MakeTable(cpu, l1, l2);
        var smbios = SmbiosTable.FromRawTableData(table);

        var caches = smbios.GetCachesFor(smbios.ProcessorInformation.First());
        Assert.Equal(2, caches.Count);
    }

    [Fact]
    public void GetCachesFor_HandleNotInTable_SkipsGracefully()
    {
        // L2CacheHandle points to a handle that doesn't exist in the table.
        var cpu = MakeCpuWithCacheHandles(0x0004, 0x0007, 0x00FF, 0xFFFF);
        var l1  = MakeStructure(7, 0x0007, MakeCachePayload(), new[] { "L1" });

        var table  = MakeTable(cpu, l1);
        var smbios = SmbiosTable.FromRawTableData(table);

        var caches = smbios.GetCachesFor(smbios.ProcessorInformation.First());
        Assert.Single(caches); // only L1 resolved; dangling L2 handle silently skipped
    }
}
