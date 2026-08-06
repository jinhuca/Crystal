using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.HardwareFeatures.SerialPortConfiguration;

public static class WmiSerialPortConfigurationExtensions
{
    public static async Task<IReadOnlyList<SerialPortConfigurationMetrics>> ToSafeSerialPortConfigurationMetricsAsync(
        this IWmiHardwareProvider provider,
        CancellationToken cancellationToken)
    {
        try
        {
            var rows = await provider.GetMultiMetricsForClassAsync(WmiSerialPortConfiguration.ClassName, cancellationToken);
            if (rows == null || rows.Count == 0) return Array.Empty<SerialPortConfigurationMetrics>();

            var results = new List<SerialPortConfigurationMetrics>(rows.Count);
            foreach (var data in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;
                int? I(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
                bool? B(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;

                results.Add(new SerialPortConfigurationMetrics(
                    S(WmiSerialPortConfiguration.Caption),
                    S(WmiSerialPortConfiguration.Description),
                    S(WmiSerialPortConfiguration.Name),
                    S(WmiSerialPortConfiguration.SettingID),
                    B(WmiSerialPortConfiguration.AbortReadWriteOnError),
                    (uint?)I(WmiSerialPortConfiguration.BaudRate),
                    B(WmiSerialPortConfiguration.Binary),
                    (uint?)I(WmiSerialPortConfiguration.BitsPerByte),
                    B(WmiSerialPortConfiguration.ContinueXMitOnXOff),
                    B(WmiSerialPortConfiguration.CTSOutflowControl),
                    B(WmiSerialPortConfiguration.DiscardNULL),
                    B(WmiSerialPortConfiguration.DSROutflowControl),
                    B(WmiSerialPortConfiguration.DSRSensitivity),
                    (uint?)I(WmiSerialPortConfiguration.DTRFlowControlType),
                    (uint?)I(WmiSerialPortConfiguration.EOFCharacter),
                    (uint?)I(WmiSerialPortConfiguration.ErrorReplaceCharacter),
                    (uint?)I(WmiSerialPortConfiguration.InFlowControlType),
                    (uint?)I(WmiSerialPortConfiguration.OutFlowControlType),
                    (uint?)I(WmiSerialPortConfiguration.Parity),
                    B(WmiSerialPortConfiguration.ParityCheck),
                    (uint?)I(WmiSerialPortConfiguration.RTSFlowControlType),
                    (uint?)I(WmiSerialPortConfiguration.StopBits),
                    (uint?)I(WmiSerialPortConfiguration.XOffCharacter),
                    (uint?)I(WmiSerialPortConfiguration.XOffXMitThreshold),
                    (uint?)I(WmiSerialPortConfiguration.XOnCharacter),
                    (uint?)I(WmiSerialPortConfiguration.XOnXMitThreshold),
                    B(WmiSerialPortConfiguration.XOnXOffInFlowControl),
                    B(WmiSerialPortConfiguration.XOnXOffOutFlowControl)));
            }

            return results;
        }
        catch
        {
            return Array.Empty<SerialPortConfigurationMetrics>();
        }
    }
}
