using System;
using System.Linq;
using Crystal.Provider.Smbios.Structures;
using Crystal.Provider.Smbios.Types;
using Xunit;
using static Crystal.Provider.Smbios.Tests.TestHelpers;

namespace Crystal.Provider.Smbios.Tests;

public class PhysicalMemoryArrayTests
{
    private static byte[] MakeArrayPayload(
        MemoryArrayLocation location = MemoryArrayLocation.SystemBoard,
        MemoryArrayUse use = MemoryArrayUse.SystemMemory,
        MemoryErrorCorrection ecc = MemoryErrorCorrection.None,
        uint maxCapacityKiB = 0,
        ushort errorHandle = 0xFFFE,
        ushort numDevices = 4,
        ulong extendedCapacityBytes = 0)
    {
        bool useExtended = maxCapacityKiB == 0x80000000;
        int length = useExtended ? 0x17 : 0x0F;
        var payload = new byte[length - 4];

        payload[0x00] = (byte)location;
        payload[0x01] = (byte)use;
        payload[0x02] = (byte)ecc;
        BitConverter.GetBytes(maxCapacityKiB).CopyTo(payload, 0x03);
        payload[0x07] = (byte)errorHandle;
        payload[0x08] = (byte)(errorHandle >> 8);
        payload[0x09] = (byte)numDevices;
        payload[0x0A] = (byte)(numDevices >> 8);

        if (useExtended)
            BitConverter.GetBytes(extendedCapacityBytes).CopyTo(payload, 0x0B);

        return payload;
    }

    [Fact]
    public void Decode_LegacyCapacity_ReturnsKiBDirectly()
    {
        var payload = MakeArrayPayload(maxCapacityKiB: 33_554_432); // 32 GiB in KiB
        var table   = MakeTable(MakeStructure(16, 0x0002, payload));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(33_554_432L, smbios.PhysicalMemoryArrays.First().MaxCapacityKiB);
    }

    [Fact]
    public void Decode_ExtendedCapacity_UsedWhenLegacyIsSentinel()
    {
        ulong bytes = 128L * 1024 * 1024 * 1024; // 128 GiB expressed in bytes
        var payload = MakeArrayPayload(maxCapacityKiB: 0x80000000, extendedCapacityBytes: bytes);
        var table   = MakeTable(MakeStructure(16, 0x0002, payload));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(128L * 1024 * 1024, smbios.PhysicalMemoryArrays.First().MaxCapacityKiB); // KiB
    }

    [Fact]
    public void Decode_Location_And_Use_DecodedCorrectly()
    {
        var payload = MakeArrayPayload(location: MemoryArrayLocation.SystemBoard, use: MemoryArrayUse.SystemMemory);
        var table   = MakeTable(MakeStructure(16, 0x0002, payload));
        var smbios  = SmbiosTable.FromRawTableData(table);

        var array = smbios.PhysicalMemoryArrays.First();
        Assert.Equal(MemoryArrayLocation.SystemBoard, array.Location);
        Assert.Equal(MemoryArrayUse.SystemMemory, array.Use);
    }

    [Fact]
    public void Decode_ErrorCorrection_DecodedCorrectly()
    {
        var payload = MakeArrayPayload(ecc: MemoryErrorCorrection.MultiBitEcc);
        var table   = MakeTable(MakeStructure(16, 0x0002, payload));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(MemoryErrorCorrection.MultiBitEcc, smbios.PhysicalMemoryArrays.First().ErrorCorrection);
    }

    [Fact]
    public void Decode_NumberOfMemoryDevices_MatchesSlotCount()
    {
        var payload = MakeArrayPayload(numDevices: 8);
        var table   = MakeTable(MakeStructure(16, 0x0002, payload));
        var smbios  = SmbiosTable.FromRawTableData(table);

        Assert.Equal(8, smbios.PhysicalMemoryArrays.First().NumberOfMemoryDevices);
    }

    [Fact]
    public void GetArrayFor_ResolvesParentArray()
    {
        var arrayStruct = MakeStructure(16, 0x0002, MakeArrayPayload());

        var devicePayload = new byte[0x28 - 4];
        devicePayload[0x00] = 0x02; devicePayload[0x01] = 0x00; // PhysicalArrayHandle = 0x0002
        var deviceStruct = MakeStructure(17, 0x0011, devicePayload);

        var table  = MakeTable(arrayStruct, deviceStruct);
        var smbios = SmbiosTable.FromRawTableData(table);

        var device = smbios.MemoryDevices.First();
        var array  = smbios.GetArrayFor(device);

        Assert.NotNull(array);
        Assert.Equal(0x0002, array!.Handle);
    }

    [Fact]
    public void GetArrayFor_UnresolvedHandle_ReturnsNull()
    {
        var devicePayload = new byte[0x28 - 4];
        devicePayload[0x00] = 0xFF; devicePayload[0x01] = 0xFF; // dangling handle
        var deviceStruct = MakeStructure(17, 0x0011, devicePayload);

        var table  = MakeTable(deviceStruct); // no Type 16 present
        var smbios = SmbiosTable.FromRawTableData(table);

        Assert.Null(smbios.GetArrayFor(smbios.MemoryDevices.First()));
    }

    [Fact]
    public void GetDevicesIn_ReturnsAllSlotsForArray()
    {
        var arrayStruct = MakeStructure(16, 0x0002, MakeArrayPayload());

        static byte[] MakeDevice(ushort handle)
        {
            var p = new byte[0x28 - 4];
            p[0x00] = 0x02; p[0x01] = 0x00; // all point to array 0x0002
            return MakeStructure(17, handle, p);
        }

        var table  = MakeTable(arrayStruct, MakeDevice(0x10), MakeDevice(0x11), MakeDevice(0x12));
        var smbios = SmbiosTable.FromRawTableData(table);

        var array = smbios.PhysicalMemoryArrays.First();
        Assert.Equal(3, smbios.GetDevicesIn(array).Count());
    }

    [Fact]
    public void GetDevicesIn_ExcludesDevicesFromOtherArrays()
    {
        var array1 = MakeStructure(16, 0x0002, MakeArrayPayload());
        var array2 = MakeStructure(16, 0x0003, MakeArrayPayload());

        static byte[] MakeDevice(ushort handle, ushort arrayHandle)
        {
            var p = new byte[0x28 - 4];
            p[0x00] = (byte)arrayHandle; p[0x01] = (byte)(arrayHandle >> 8);
            return MakeStructure(17, handle, p);
        }

        var table  = MakeTable(
            array1, array2,
            MakeDevice(0x10, 0x0002),
            MakeDevice(0x11, 0x0002),
            MakeDevice(0x12, 0x0003));
        var smbios = SmbiosTable.FromRawTableData(table);

        var arrays = smbios.PhysicalMemoryArrays.ToList();
        Assert.Equal(2, smbios.GetDevicesIn(arrays[0]).Count());
        Assert.Single(smbios.GetDevicesIn(arrays[1]));
    }
}
