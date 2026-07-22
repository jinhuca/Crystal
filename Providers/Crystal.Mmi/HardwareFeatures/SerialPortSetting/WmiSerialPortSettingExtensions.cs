using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.SerialPortSetting;

public static class WmiSerialPortSettingExtensions
{
    public static async Task<IReadOnlyList<SerialPortSettingMetrics>> ToSafeSerialPortSettingMetricsAsync(
        this IWmiHardwareProvider provider,
        CancellationToken cancellationToken)
    {
        try
        {
            var rows = await provider.GetMultiMetricsForClassAsync(WmiSerialPortSetting.ClassName, cancellationToken);
            if (rows == null || rows.Count == 0) return Array.Empty<SerialPortSettingMetrics>();

            var results = new List<SerialPortSettingMetrics>(rows.Count);
            foreach (var data in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;

                results.Add(new SerialPortSettingMetrics(
                    S(WmiSerialPortSetting.Element),
                    S(WmiSerialPortSetting.Setting)));
            }

            return results;
        }
        catch
        {
            return Array.Empty<SerialPortSettingMetrics>();
        }
    }
}
