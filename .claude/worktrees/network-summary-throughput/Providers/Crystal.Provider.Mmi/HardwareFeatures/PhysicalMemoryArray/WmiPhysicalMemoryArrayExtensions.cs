using Crystal.Provider.Mmi.MmiEngine;
namespace Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemoryArray;
public static class WmiPhysicalMemoryArrayExtensions
{
    public static async Task<IReadOnlyList<PhysicalMemoryArrayMetrics>> ToSafePhysicalMemoryArrayMetricsAsync(this IWmiHardwareProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await provider.GetMultiMetricsForClassAsync(WmiPhysicalMemoryArray.ClassName, cancellationToken);
            if (rows == null || rows.Count == 0) return Array.Empty<PhysicalMemoryArrayMetrics>();
            var results = new List<PhysicalMemoryArrayMetrics>(rows.Count);
            foreach (var data in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;
                int? I(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
                ulong? U64(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.ULong ? v.AsReadOnlyULong() : null;
                bool? B(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
                DateTime? D(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
                float? F(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? (float)v.AsInt() : null;
                results.Add(new PhysicalMemoryArrayMetrics((ushort?)I(WmiPhysicalMemoryArray.Attributes), S(WmiPhysicalMemoryArray.Caption), S(WmiPhysicalMemoryArray.CreationClassName), (ushort?)I(WmiPhysicalMemoryArray.Depth), S(WmiPhysicalMemoryArray.Description), (ushort?)I(WmiPhysicalMemoryArray.Height), B(WmiPhysicalMemoryArray.HotSwappable), D(WmiPhysicalMemoryArray.InstallationDate), (ushort?)I(WmiPhysicalMemoryArray.Location), S(WmiPhysicalMemoryArray.Manufacturer), (uint?)I(WmiPhysicalMemoryArray.MaxCapacity), U64(WmiPhysicalMemoryArray.MaxCapacityEx), (ushort?)I(WmiPhysicalMemoryArray.MemoryDevices), (ushort?)I(WmiPhysicalMemoryArray.MemoryErrorCorrection), S(WmiPhysicalMemoryArray.Model), S(WmiPhysicalMemoryArray.Name), S(WmiPhysicalMemoryArray.OtherIdentifyingInfo), S(WmiPhysicalMemoryArray.PartNumber), B(WmiPhysicalMemoryArray.PoweredOn), B(WmiPhysicalMemoryArray.Removable), B(WmiPhysicalMemoryArray.Replaceable), S(WmiPhysicalMemoryArray.SerialNumber), S(WmiPhysicalMemoryArray.SKU), S(WmiPhysicalMemoryArray.Status), S(WmiPhysicalMemoryArray.Tag), (ushort?)I(WmiPhysicalMemoryArray.Use), S(WmiPhysicalMemoryArray.Version), F(WmiPhysicalMemoryArray.Weight), F(WmiPhysicalMemoryArray.Width)));
            }
            return results;
        }
        catch { return Array.Empty<PhysicalMemoryArrayMetrics>(); }
    }
}
