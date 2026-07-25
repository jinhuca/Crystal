using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.Directory;
public static class WmiDirectoryExtensions {
  private const string WmiClassName = WmiDirectory.ClassName;

  public static async Task<IReadOnlyList<DirectoryMetrics>> ToSafeDirectoryMetricsAsync(
    this IWmiHardwareProvider provider,
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance directory entry data blocks asynchronously
      var instancesData = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      if (instancesData == null || instancesData.Count == 0) {
        return Array.Empty<DirectoryMetrics>();
      }

      var results = new List<DirectoryMetrics>(instancesData.Count);

      // 2. Loop through every detected directory entry instance sequentially
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
        ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong
          ? v.AsReadOnlyULong() : null;

        // --- MAP ALPHABETICALLY TO INSTANCE DATA SLICE ---
        results.Add(new DirectoryMetrics(
          AccessMask: (uint?)GetInt(WmiDirectory.AccessMask),
          Archive: GetBool(WmiDirectory.Archive),
          Caption: GetStr(WmiDirectory.Caption),
          Compressed: GetBool(WmiDirectory.Compressed),
          CompressionMethod: GetStr(WmiDirectory.CompressionMethod),
          CreationClassName: GetStr(WmiDirectory.CreationClassName),
          CreationDate: GetDate(WmiDirectory.CreationDate),
          CSCreationClassName: GetStr(WmiDirectory.CSCreationClassName),
          CSName: GetStr(WmiDirectory.CSName),
          Description: GetStr(WmiDirectory.Description),
          Drive: GetStr(WmiDirectory.Drive),
          EightDotThreeFileName: GetStr(WmiDirectory.EightDotThreeFileName),
          Encrypted: GetBool(WmiDirectory.Encrypted),
          EncryptionMethod: GetStr(WmiDirectory.EncryptionMethod),
          Extension: GetStr(WmiDirectory.Extension),
          FileName: GetStr(WmiDirectory.FileName),
          FileSize: GetULong(WmiDirectory.FileSize),
          FileType: GetStr(WmiDirectory.FileType),
          FSCreationClassName: GetStr(WmiDirectory.FSCreationClassName),
          FSName: GetStr(WmiDirectory.FSName),
          Hidden: GetBool(WmiDirectory.Hidden),
          InstallDate: GetDate(WmiDirectory.InstallDate),
          InUseCount: GetULong(WmiDirectory.InUseCount),
          LastAccessed: GetDate(WmiDirectory.LastAccessed),
          LastModified: GetDate(WmiDirectory.LastModified),
          Name: GetStr(WmiDirectory.Name),
          Path: GetStr(WmiDirectory.Path),
          Readable: GetBool(WmiDirectory.Readable),
          Status: GetStr(WmiDirectory.Status),
          System: GetBool(WmiDirectory.System),
          Writeable: GetBool(WmiDirectory.Writeable)));
      }
      return results;
    }
    catch {
      return Array.Empty<DirectoryMetrics>();
    }
  }
}
