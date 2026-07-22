using Crystal.Mmi.HardwareFeatures.CacheMemory;
using Crystal.Mmi.HardwareFeatures.PhysicalMemory;
using Crystal.Mmi.HardwareFeatures.PhysicalMemoryArray;
using Crystal.Mmi.HardwareFeatures.SystemSlot;
using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.Cli;

public static class CliDemos
{
    public static async Task DumpPhysicalMemoryArraysAsync(
        IWmiHardwareProvider provider,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(output);

        output.WriteLine();
        output.WriteLine("========== Physical Memory Arrays ==========");
        output.WriteLine();

        var arrays = await provider.ToSafePhysicalMemoryArrayMetricsAsync(cancellationToken);

        foreach (var array in arrays)
        {
            output.WriteLine($"Name: {Format(array.Name)}");
            output.WriteLine($"Manufacturer: {Format(array.Manufacturer)}");
            output.WriteLine($"Location: {Format(array.LocationName)}");
            output.WriteLine($"Use: {Format(array.UseName)}");
            output.WriteLine($"Memory Devices: {Format(array.MemoryDevices)}");
            output.WriteLine($"Max Capacity: {Format(array.MaxCapacityExInGB)} GB");
            output.WriteLine($"ECC: {Format(array.MemoryErrorCorrectionName)}");
            output.WriteLine($"Status: {Format(array.Status)}");
            output.WriteLine();
        }
    }

    public static async Task DumpCacheMemoryAsync(
        IWmiHardwareProvider provider,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(output);

        output.WriteLine();
        output.WriteLine("========== Cache Memory ==========");
        output.WriteLine();

        var caches = await provider.ToSafeCacheMemoryMetricsAsync(cancellationToken);

        foreach (var cache in caches)
        {
            output.WriteLine($"Name: {Format(cache.Name)}");
            output.WriteLine($"Level: {Format(cache.LevelName)}");
            output.WriteLine($"Type: {Format(cache.CacheTypeName)}");
            output.WriteLine($"Associativity: {Format(cache.AssociativityName)}");
            output.WriteLine($"Installed Size: {Format(cache.InstalledSizeInMB)} MB");
            output.WriteLine($"Max Size: {Format(cache.MaxCacheSizeInMB)} MB");
            output.WriteLine($"Line Size: {Format(cache.LineSize)} bytes");
            output.WriteLine($"Status: {Format(cache.Status)}");
            output.WriteLine();
        }
    }

    public static async Task DumpSystemSlotsAsync(
        IWmiHardwareProvider provider,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(output);

        output.WriteLine();
        output.WriteLine("========== System Slots ==========");
        output.WriteLine();

        var slots = await provider.ToSafeSystemSlotMetricsAsync(cancellationToken);

        foreach (var slot in slots)
        {
            output.WriteLine($"Designation: {Format(slot.SlotDesignation)}");
            output.WriteLine($"Name: {Format(slot.Name)}");
            output.WriteLine($"Usage: {Format(slot.CurrentUsageName)}");
            output.WriteLine($"Width: {Format(slot.SlotWidthName)}");
            output.WriteLine($"Manufacturer: {Format(slot.Manufacturer)}");
            output.WriteLine($"PME Signal: {Format(slot.PMESignal)}");
            output.WriteLine($"Hot Plug: {Format(slot.SupportsHotPlug)}");
            output.WriteLine($"Status: {Format(slot.Status)}");
            output.WriteLine();
        }
    }

    public static async Task DumpMemoryTopologyAsync(
        IWmiHardwareProvider provider,
        TextWriter output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(output);

        output.WriteLine();
        output.WriteLine("========== Memory Topology ==========");
        output.WriteLine();

        var arrays = await provider.ToSafePhysicalMemoryArrayMetricsAsync(cancellationToken);
        var dimms = await provider.ToSafePhysicalMemoryMetricsAsync(cancellationToken);
        var caches = await provider.ToSafeCacheMemoryMetricsAsync(cancellationToken);

        var totalRamGb = dimms.Sum(x=>x.CapacityInGB ?? 0);

    output.WriteLine($"Arrays Found: {arrays.Count}");
        output.WriteLine($"DIMM Count: {dimms.Count}");
        output.WriteLine($"Installed RAM: {totalRamGb:N1} GB");

        foreach (var cache in caches.OrderBy(x => x.Level))
        {
            output.WriteLine($"  {Format(cache.LevelName)}: {Format(cache.InstalledSizeInMB)} MB");
        }

        output.WriteLine();
    }

    private static string Format(object? value)
    {
        return value switch
        {
            null => "-",
            string s when string.IsNullOrWhiteSpace(s) => "-",
            double d => d.ToString("N2"),
            float f => f.ToString("N2"),
            _ => value.ToString() ?? "-"
        };
    }
}
