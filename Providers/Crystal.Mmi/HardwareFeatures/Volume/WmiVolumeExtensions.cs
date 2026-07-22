using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.Volume;

public static class WmiVolumeExtensions
{
    public static async Task<IReadOnlyList<VolumeMetrics>> ToSafeVolumeMetricsAsync(this IWmiHardwareProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var instancesData = await provider.GetMultiMetricsForClassAsync(WmiVolume.ClassName, cancellationToken);
            if (instancesData == null || instancesData.Count == 0) return Array.Empty<VolumeMetrics>();
            var results = new List<VolumeMetrics>(instancesData.Count);

            foreach (var data in instancesData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;
                int? I(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
                uint? U32(string k) => I(k) is int i && i >= 0 ? (uint)i : data.TryGetValue(k, out var v) && v.Type == WmiType.ULong && v.AsReadOnlyULong() <= uint.MaxValue ? (uint)v.AsReadOnlyULong() : null;
                ushort? U16(string k) => U32(k) is uint u && u <= ushort.MaxValue ? (ushort)u : null;
                ulong? U64(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.ULong ? v.AsReadOnlyULong() : null;
                bool? B(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
                DateTime? D(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
                ushort[]? U16A(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.UShortArray ? v.AsUShortArray() : null;

                results.Add(new VolumeMetrics(
                    S(WmiVolume.Name), S(WmiVolume.Caption), S(WmiVolume.Description), S(WmiVolume.DeviceID),
                    U64(WmiVolume.Capacity), U64(WmiVolume.FreeSpace), U32(WmiVolume.BlockSize),
                    S(WmiVolume.DriveLetter), U16(WmiVolume.DriveType), S(WmiVolume.Label), S(WmiVolume.FileSystem), S(WmiVolume.SerialNumber),
                    B(WmiVolume.Automount), B(WmiVolume.BootVolume), B(WmiVolume.SystemVolume), B(WmiVolume.Compressed), B(WmiVolume.DirtyBitSet), B(WmiVolume.IndexingEnabled), B(WmiVolume.PageFilePresent),
                    B(WmiVolume.QuotasEnabled), B(WmiVolume.QuotasIncomplete), B(WmiVolume.QuotasRebuilding), B(WmiVolume.SupportsDiskQuotas), B(WmiVolume.SupportsFileBasedCompression),
                    U16(WmiVolume.Availability), U32(WmiVolume.ConfigManagerErrorCode), B(WmiVolume.ConfigManagerUserConfig),
                    S(WmiVolume.CreationClassName), B(WmiVolume.ErrorCleared), S(WmiVolume.ErrorDescription), S(WmiVolume.ErrorMethodology),
                    D(WmiVolume.InstallationDate), U32(WmiVolume.LastErrorCode), S(WmiVolume.PNPDeviceID), U16A(WmiVolume.PowerManagementCapabilities), B(WmiVolume.PowerManagementSupported),
                    S(WmiVolume.Purpose), S(WmiVolume.Status), U16(WmiVolume.StatusInfo), S(WmiVolume.SystemCreationClassName), S(WmiVolume.SystemName), U32(WmiVolume.MaximumFileNameLength)));
            }
            return results;
        }
        catch { return Array.Empty<VolumeMetrics>(); }
    }
}
