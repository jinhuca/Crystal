using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.HardwareFeatures.Printer;
public static class WmiPrinterExtensions {
  private const string WmiClassName = WmiPrinter.ClassName;

  public static async Task<IReadOnlyList<PrinterMetrics>> ToSafePrinterMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance printer data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<PrinterMetrics>();
      }

      var results = new List<PrinterMetrics>(instancesData.Count);

      // 2. Loop through every single detected printer device
      foreach (var data in instancesData) {
        cancellationToken.ThrowIfCancellationRequested();

        // --- CLEAN DATA EXTRACTION HELPERS ---
        string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String
          ? v.AsString() : null;
        int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int
          ? v.AsInt() : null;
        bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool
          ? v.AsBool() : null;
        DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime
          ? v.AsDateTime() : null;
        ushort[]? GetUShortArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.UShortArray
          ? v.AsUShortArray() : null;
        string[]? GetStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray
          ? v.AsStringArray() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new PrinterMetrics(
          Attributes: (uint?)GetInt(WmiPrinter.Attributes),
          Availability: (ushort?)GetInt(WmiPrinter.Availability),
          AveragePagesPerMinute: (uint?)GetInt(WmiPrinter.AveragePagesPerMinute),
          Caption: GetStr(WmiPrinter.Caption),
          Comment: GetStr(WmiPrinter.Comment),
          CreationClassName: GetStr(WmiPrinter.CreationClassName),
          Default: GetBool(WmiPrinter.Default),
          DefaultPriority: (uint?)GetInt(WmiPrinter.DefaultPriority),
          Description: GetStr(WmiPrinter.Description),
          DetectedErrorState: (ushort?)GetInt(WmiPrinter.DetectedErrorState),
          DeviceID: GetStr(WmiPrinter.DeviceID),
          Direct: GetBool(WmiPrinter.Direct),
          DoCompleteFirst: GetBool(WmiPrinter.DoCompleteFirst),
          DriverName: GetStr(WmiPrinter.DriverName),
          EnableBIDI: GetBool(WmiPrinter.EnableBIDI),
          EnableDevQueryPrint: GetBool(WmiPrinter.EnableDevQueryPrint),
          ErrorCleared: GetBool(WmiPrinter.ErrorCleared),
          ErrorDescription: GetStr(WmiPrinter.ErrorDescription),
          ExtendedDetectedErrorState: (ushort?)GetInt(WmiPrinter.ExtendedDetectedErrorState),
          ExtendedPrinterStatus: (ushort?)GetInt(WmiPrinter.ExtendedPrinterStatus),
          Hidden: GetBool(WmiPrinter.Hidden),
          HorizontalResolution: (uint?)GetInt(WmiPrinter.HorizontalResolution),
          InstallDate: GetDate(WmiPrinter.InstallDate),
          JobCountSinceLastReset: (uint?)GetInt(WmiPrinter.JobCountSinceLastReset),
          KeepPrintedJobs: GetBool(WmiPrinter.KeepPrintedJobs),
          LastErrorCode: (uint?)GetInt(WmiPrinter.LastErrorCode),
          Local: GetBool(WmiPrinter.Local),
          Location: GetStr(WmiPrinter.Location),
          MaxCopies: (uint?)GetInt(WmiPrinter.MaxCopies),
          MaxNumberUp: (uint?)GetInt(WmiPrinter.MaxNumberUp),
          MaxSizeSupported: (uint?)GetInt(WmiPrinter.MaxSizeSupported),
          Name: GetStr(WmiPrinter.Name),
          Network: GetBool(WmiPrinter.Network),
          PaperSizesSupported: GetUShortArr(WmiPrinter.PaperSizesSupported),
          PortName: GetStr(WmiPrinter.PortName),
          PrinterPaperNames: GetStrArr(WmiPrinter.PrinterPaperNames),
          PrinterState: (uint?)GetInt(WmiPrinter.PrinterState),
          PrinterStatus: (ushort?)GetInt(WmiPrinter.PrinterStatus),
          PrintJobDataType: GetStr(WmiPrinter.PrintJobDataType),
          PrintProcessor: GetStr(WmiPrinter.PrintProcessor),
          Priority: (uint?)GetInt(WmiPrinter.Priority),
          Published: GetBool(WmiPrinter.Published),
          Queued: GetBool(WmiPrinter.Queued),
          RawOnly: GetBool(WmiPrinter.RawOnly),
          SeparatorFile: GetStr(WmiPrinter.SeparatorFile),
          ServerName: GetStr(WmiPrinter.ServerName),
          ShareName: GetStr(WmiPrinter.ShareName),
          Shared: GetBool(WmiPrinter.Shared),
          SpoolEnabled: GetBool(WmiPrinter.SpoolEnabled),
          StartTime: GetDate(WmiPrinter.StartTime),
          Status: GetStr(WmiPrinter.Status),
          StatusInfo: (ushort?)GetInt(WmiPrinter.StatusInfo),
          SystemCreationClassName: GetStr(WmiPrinter.SystemCreationClassName),
          SystemName: GetStr(WmiPrinter.SystemName),
          TimeOfLastReset: GetDate(WmiPrinter.TimeOfLastReset),
          UntilTime: GetDate(WmiPrinter.UntilTime),
          VerticalResolution: (uint?)GetInt(WmiPrinter.VerticalResolution),
          WorkOffline: GetBool(WmiPrinter.WorkOffline)));
      }
      return results;
    }
    catch {
      return Array.Empty<PrinterMetrics>();
    }
  }
}
