using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.SMBIOSMemory;

public static class WmiSMBIOSMemoryExtensions
{
    public static async Task<IReadOnlyList<SMBIOSMemoryMetrics>> ToSafeSMBIOSMemoryMetricsAsync(
        this IWmiHardwareProvider provider,
        CancellationToken cancellationToken)
    {
        try
        {
            var rows = await provider.GetMultiMetricsForClassAsync(WmiSMBIOSMemory.ClassName, cancellationToken);
            if (rows == null || rows.Count == 0) return Array.Empty<SMBIOSMemoryMetrics>();

            var results = new List<SMBIOSMemoryMetrics>(rows.Count);
            foreach (var data in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;
                int? I(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
                bool? B(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
                DateTime? D(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
                ushort[]? U16A(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.UShortArray ? v.AsUShortArray() : null;
                ulong? U64(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.ULong ? v.AsReadOnlyULong() : null;

                results.Add(new SMBIOSMemoryMetrics(
                    (ushort?)I(WmiSMBIOSMemory.Access),
                    S(WmiSMBIOSMemory.AdditionalErrorData),
                    (ushort?)I(WmiSMBIOSMemory.Availability),
                    U64(WmiSMBIOSMemory.BlockSize),
                    S(WmiSMBIOSMemory.Caption),
                    (uint?)I(WmiSMBIOSMemory.ConfigManagerErrorCode),
                    B(WmiSMBIOSMemory.ConfigManagerUserConfig),
                    B(WmiSMBIOSMemory.CorrectableError),
                    S(WmiSMBIOSMemory.CreationClassName),
                    S(WmiSMBIOSMemory.Description),
                    U64(WmiSMBIOSMemory.EndingAddress),
                    (ushort?)I(WmiSMBIOSMemory.ErrorAccess),
                    U64(WmiSMBIOSMemory.ErrorAddress),
                    B(WmiSMBIOSMemory.ErrorCleared),
                    (ushort?)I(WmiSMBIOSMemory.ErrorCorrectType),
                    S(WmiSMBIOSMemory.ErrorData),
                    (ushort?)I(WmiSMBIOSMemory.ErrorDataOrder),
                    S(WmiSMBIOSMemory.ErrorDescription),
                    (ushort?)I(WmiSMBIOSMemory.ErrorInfo),
                    S(WmiSMBIOSMemory.ErrorMethodology),
                    U64(WmiSMBIOSMemory.ErrorResolution),
                    D(WmiSMBIOSMemory.ErrorTime),
                    (uint?)I(WmiSMBIOSMemory.ErrorTransferSize),
                    D(WmiSMBIOSMemory.InstallDate),
                    (uint?)I(WmiSMBIOSMemory.LastErrorCode),
                    S(WmiSMBIOSMemory.Name),
                    U64(WmiSMBIOSMemory.NumberOfBlocks),
                    S(WmiSMBIOSMemory.OtherErrorDescription),
                    S(WmiSMBIOSMemory.PNPDeviceID),
                    U16A(WmiSMBIOSMemory.PowerManagementCapabilities),
                    B(WmiSMBIOSMemory.PowerManagementSupported),
                    S(WmiSMBIOSMemory.Purpose),
                    U64(WmiSMBIOSMemory.StartingAddress),
                    S(WmiSMBIOSMemory.Status),
                    (ushort?)I(WmiSMBIOSMemory.StatusInfo),
                    S(WmiSMBIOSMemory.SystemCreationClassName),
                    B(WmiSMBIOSMemory.SystemLevelAddress),
                    S(WmiSMBIOSMemory.SystemName)));
            }

            return results;
        }
        catch
        {
            return Array.Empty<SMBIOSMemoryMetrics>();
        }
    }
}
