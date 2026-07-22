using Crystal.Mmi.MmiEngine;
namespace Crystal.Mmi.HardwareFeatures.SystemSlot;
public static class WmiSystemSlotExtensions
{
    public static async Task<IReadOnlyList<SystemSlotMetrics>> ToSafeSystemSlotMetricsAsync(this IWmiHardwareProvider provider, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await provider.GetMultiMetricsForClassAsync(WmiSystemSlot.ClassName, cancellationToken);
            if (rows == null || rows.Count == 0) return Array.Empty<SystemSlotMetrics>();
            var results = new List<SystemSlotMetrics>(rows.Count);
            foreach (var data in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string? S(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.String ? v.AsString() : null;
                int? I(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? v.AsInt() : null;
                bool? B(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Bool ? v.AsBool() : null;
                DateTime? D(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.DateTime ? v.AsDateTime() : null;
                ushort[]? A(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.UShortArray ? v.AsUShortArray() : null;
                float? F(string k) => data.TryGetValue(k, out var v) && v.Type == WmiType.Int ? (float)v.AsInt() : null;
                results.Add(new SystemSlotMetrics(S(WmiSystemSlot.Caption), S(WmiSystemSlot.ConnectorPinout), A(WmiSystemSlot.ConnectorType), S(WmiSystemSlot.CreationClassName), (ushort?)I(WmiSystemSlot.CurrentUsage), S(WmiSystemSlot.Description), F(WmiSystemSlot.HeightAllowed), D(WmiSystemSlot.InstallationDate), F(WmiSystemSlot.LengthAllowed), S(WmiSystemSlot.Manufacturer), (ushort?)I(WmiSystemSlot.MaxDataWidth), S(WmiSystemSlot.Model), S(WmiSystemSlot.Name), (ushort?)I(WmiSystemSlot.Number), S(WmiSystemSlot.OtherIdentifyingInfo), S(WmiSystemSlot.PartNumber), B(WmiSystemSlot.PMESignal), B(WmiSystemSlot.PoweredOn), S(WmiSystemSlot.PurposeDescription), S(WmiSystemSlot.SerialNumber), B(WmiSystemSlot.Shared), S(WmiSystemSlot.SKU), S(WmiSystemSlot.SlotDesignation), B(WmiSystemSlot.SpecialPurpose), S(WmiSystemSlot.Status), B(WmiSystemSlot.SupportsHotPlug), S(WmiSystemSlot.Tag), (uint?)I(WmiSystemSlot.ThermalRating), A(WmiSystemSlot.VccMixedVoltageSupport), S(WmiSystemSlot.Version), A(WmiSystemSlot.VppMixedVoltageSupport)));
            }
            return results;
        }
        catch { return Array.Empty<SystemSlotMetrics>(); }
    }
}
