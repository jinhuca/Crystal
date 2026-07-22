using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.SoftwareFeatures.OperatingSystem;

public static class WmiOperatingSystemExtensions {
  private const string WmiClassName = WmiOperatingSystem.ClassName;

  public static async Task<OperatingSystemMetrics> ToSafeOperatingSystemMetricsAsync(
    this IWmiHardwareProvider provider, 
    CancellationToken cancellationToken) {
    try {
      // 1. Fetch multi-instance data collection asynchronously
      var instances = await provider.GetMultiMetricsForClassAsync(WmiClassName, cancellationToken);
      var data = instances.FirstOrDefault();

      // --- FULL NULL/CRASH FALLBACK RETRIEVAL ---
      if (data == null || data.Count == 0) {
        return new OperatingSystemMetrics(
          null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
          null, null, null, null, null, null, null);
      }

      cancellationToken.ThrowIfCancellationRequested();

      // --- CLEAN LOOKUP CONDITIONAL WRAPPERS ---
      string? GetStr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.String 
        ? v.AsString() : null;
      int? GetInt(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Int 
        ? v.AsInt() : null;
      bool? GetBool(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.Bool 
        ? v.AsBool() : null;
      DateTime? GetDate(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.DateTime 
        ? v.AsDateTime() : null;
      string[]? GetStrArr(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.StringArray 
        ? v.AsStringArray() : null;
      ulong? GetULong(string key) => data.TryGetValue(key, out var v) && v.Type == WmiType.ULong 
        ? v.AsReadOnlyULong() : null;

      // --- INSTANTIATE SORTED EXTRACTED VALUES ---
      return new OperatingSystemMetrics(
        BootDevice: (uint?)GetInt(WmiOperatingSystem.BootDevice),
        BuildNumber: GetStr(WmiOperatingSystem.BuildNumber),
        BuildType: GetStr(WmiOperatingSystem.BuildType),
        Caption: GetStr(WmiOperatingSystem.Caption),
        CodeSet: GetStr(WmiOperatingSystem.CodeSet),
        CountryCode: GetStr(WmiOperatingSystem.CountryCode),
        CreationClassName: GetStr(WmiOperatingSystem.CreationClassName),
        CSCreationClassName: GetStr(WmiOperatingSystem.CSCreationClassName),
        CSDVersion: GetStr(WmiOperatingSystem.CSDVersion),
        CSName: GetStr(WmiOperatingSystem.CSName),
        CurrentTimeZone: (short?)GetInt(WmiOperatingSystem.CurrentTimeZone),
        DataExecutionPrevention_Available: GetBool(WmiOperatingSystem.DataExecutionPrevention_Available),
        DataExecutionPrevention_32BitApplications: GetBool(WmiOperatingSystem.DataExecutionPrevention_32BitApplications),
        DataExecutionPrevention_Drivers: GetBool(WmiOperatingSystem.DataExecutionPrevention_Drivers),
        DataExecutionPrevention_SupportPolicy: (ushort?)GetInt(WmiOperatingSystem.DataExecutionPrevention_SupportPolicy),
        Distributed: GetBool(WmiOperatingSystem.Distributed),
        EncryptionLevel: (uint?)GetInt(WmiOperatingSystem.EncryptionLevel),
        ForegroundApplicationBoostScheduling: (ushort?)GetInt(WmiOperatingSystem.ForegroundApplicationBoostScheduling),
        FreePhysicalMemory: GetULong(WmiOperatingSystem.FreePhysicalMemory),
        FreeSpaceInPagingFiles: GetULong(WmiOperatingSystem.FreeSpaceInPagingFiles),
        FreeVirtualMemory: GetULong(WmiOperatingSystem.FreeVirtualMemory),
        InstallationDate: GetDate(WmiOperatingSystem.InstallationDate),
        LargeSystemCache: (uint?)GetInt(WmiOperatingSystem.LargeSystemCache),
        LastBootUpTime: GetDate(WmiOperatingSystem.LastBootUpTime),
        LocalDateTime: GetDate(WmiOperatingSystem.LocalDateTime),
        Locale: GetStr(WmiOperatingSystem.Locale),
        Manufacturer: GetStr(WmiOperatingSystem.Manufacturer),
        MaxNumberOfProcesses: (uint?)GetInt(WmiOperatingSystem.MaxNumberOfProcesses),
        MaxProcessMemorySize: GetULong(WmiOperatingSystem.MaxProcessMemorySize),
        MUILanguages: GetStrArr(WmiOperatingSystem.MUILanguages),
        Name: GetStr(WmiOperatingSystem.Name),
        NumberOfLicensedUsers: (uint?)GetInt(WmiOperatingSystem.NumberOfLicensedUsers),
        NumberOfProcesses: (uint?)GetInt(WmiOperatingSystem.NumberOfProcesses),
        NumberOfUsers: (uint?)GetInt(WmiOperatingSystem.NumberOfUsers),
        OperatingSystemSKU: (ushort?)GetInt(WmiOperatingSystem.OperatingSystemSKU),
        Organization: GetStr(WmiOperatingSystem.Organization),
        OSArchitecture: GetStr(WmiOperatingSystem.OSArchitecture),
        OSLanguage: (uint?)GetInt(WmiOperatingSystem.OSLanguage),
        OSProductSuite: (uint?)GetInt(WmiOperatingSystem.OSProductSuite),
        OSType: (ushort?)GetInt(WmiOperatingSystem.OSType),
        OtherTypeDescription: GetStr(WmiOperatingSystem.OtherTypeDescription),
        PAEEnabled: GetBool(WmiOperatingSystem.PAEEnabled),
        PlusID: GetStr(WmiOperatingSystem.PlusID),
        PlusProductID: (uint?)GetInt(WmiOperatingSystem.PlusProductID),
        Primary: GetBool(WmiOperatingSystem.Primary),
        ProductType: (uint?)GetInt(WmiOperatingSystem.ProductType),
        RegisteredUser: GetStr(WmiOperatingSystem.RegisteredUser),
        SerialNumber: GetStr(WmiOperatingSystem.SerialNumber),
        ServicePackMajorVersion: (ushort?)GetInt(WmiOperatingSystem.ServicePackMajorVersion),
        ServicePackMinorVersion: (ushort?)GetInt(WmiOperatingSystem.ServicePackMinorVersion),
        SizeStoredInPagingFiles: GetULong(WmiOperatingSystem.SizeStoredInPagingFiles),
        Status: GetStr(WmiOperatingSystem.Status),
        SuiteMask: (uint?)GetInt(WmiOperatingSystem.SuiteMask),
        SystemDevice: GetStr(WmiOperatingSystem.SystemDevice),
        SystemDirectory: GetStr(WmiOperatingSystem.SystemDirectory),
        SystemDrive: GetStr(WmiOperatingSystem.SystemDrive),
        TotalSwapSpaceSize: GetULong(WmiOperatingSystem.TotalSwapSpaceSize),
        TotalVirtualMemorySize: GetULong(WmiOperatingSystem.TotalVirtualMemorySize),
        TotalVisibleMemorySize: GetULong(WmiOperatingSystem.TotalVisibleMemorySize),
        Version: GetStr(WmiOperatingSystem.Version),
        WindowsDirectory: GetStr(WmiOperatingSystem.WindowsDirectory));
    }
    catch {
      return new OperatingSystemMetrics(
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null);
    }
  }
}

