namespace Crystal.Provider.Mmi.HardwareFeatures.CacheMemory;

/// <summary>
/// Represents the metrics of cache memory in a computer system.
/// </summary>
/// <param name="Access">The access type for the cache memory.</param>
/// <param name="AdditionalErrorData">Additional error data related to the cache memory.</param>
/// <param name="Associativity">The associativity of the cache memory.</param>
/// <param name="Availability">The availability status of the cache memory.</param>
/// <param name="BlockSize">The size of each block in the cache memory.</param>
/// <param name="CacheSpeed">The speed of the cache memory.</param>
/// <param name="CacheType">The type of the cache memory.</param>
/// <param name="Caption">The caption for the cache memory.</param>
/// <param name="ConfigManagerErrorCode">The error code for the configuration manager.</param>
/// <param name="ConfigManagerUserConfig">Indicates if the configuration manager is user-configured.</param>
/// <param name="CorrectableError">Indicates if the error is correctable.</param>
/// <param name="CreationClassName">The class name for the creation of the cache memory.</param>
/// <param name="CurrentSRAM">The current SRAM for the cache memory.</param>
/// <param name="Description">The description for the cache memory.</param>
/// <param name="DeviceID">The device ID for the cache memory.</param>
/// <param name="EndingAddress">The ending address for the cache memory.</param>
/// <param name="ErrorAccess">The access type for the error.</param>
/// <param name="ErrorAddress">The address for the error.</param>
/// <param name="ErrorCleared">Indicates if the error is cleared.</param>
/// <param name="ErrorCorrectType">The type of error correction.</param>
/// <param name="ErrorData">The data for the error.</param>
/// <param name="ErrorDataOrder">The order of the error data.</param>
/// <param name="ErrorDescription">The description for the error.</param>
/// <param name="ErrorInfo">The info for the error.</param>
/// <param name="ErrorMethodology">The methodology for the error.</param>
/// <param name="ErrorResolution">The resolution for the error.</param>
/// <param name="ErrorTime">The time for the error.</param>
/// <param name="ErrorTransferSize">The transfer size for the error.</param>
/// <param name="FlushTimer">The timer for flushing the cache.</param>
/// <param name="InstallDate">The date when the cache memory was installed.</param>
/// <param name="InstalledSize">The size of the installed cache memory.</param>
/// <param name="LastErrorCode">The error code for the last error.</param>
/// <param name="Level">The level of the cache memory.</param>
/// <param name="LineSize">The size of each line in the cache memory.</param>
/// <param name="Location">The location of the cache memory.</param>
/// <param name="MaxCacheSize">The maximum size of the cache memory.</param>
/// <param name="Name">The name of the cache memory.</param>
/// <param name="NumberOfBlocks">The number of blocks in the cache memory.</param>
/// <param name="OtherErrorDescription">The description for other errors.</param>
/// <param name="PNPDeviceID">The PNP device ID for the cache memory.</param>
/// <param name="PowerManagementCapabilities">The capabilities for power management.</param>
/// <param name="PowerManagementSupported">Indicates if power management is supported.</param>
/// <param name="Purpose">The purpose of the cache memory.</param>
/// <param name="ReadPolicy">The policy for reading from the cache.</param>
/// <param name="ReplacementPolicy">The policy for replacing cache entries.</param>
/// <param name="StartingAddress">The starting address for the cache memory.</param>
/// <param name="Status">The status of the cache memory.</param>
/// <param name="StatusInfo">The info for the status of the cache memory.</param>
/// <param name="SupportedSRAM">The SRAM supported by the cache memory.</param>
/// <param name="SystemCreationClassName">The class name for the creation of the system.</param>
/// <param name="SystemLevelAddress">The level address for the system.</param>
/// <param name="SystemName">The name of the system.</param>
/// <param name="WritePolicy">The policy for writing to the cache.</param>
public record CacheMemoryMetrics(
  ushort? Access,
  string? AdditionalErrorData,
  ushort? Associativity,
  ushort? Availability,
  ulong? BlockSize,
  ushort? CacheSpeed,
  ushort? CacheType,
  string? Caption,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  bool? CorrectableError,
  string? CreationClassName,
  ushort[]? CurrentSRAM,
  string? Description,
  string? DeviceID,
  ulong? EndingAddress,
  ushort? ErrorAccess,
  ulong? ErrorAddress,
  bool? ErrorCleared,
  ushort? ErrorCorrectType,
  string? ErrorData,
  ushort? ErrorDataOrder,
  string? ErrorDescription,
  ushort? ErrorInfo,
  string? ErrorMethodology,
  ulong? ErrorResolution,
  DateTime? ErrorTime,
  uint? ErrorTransferSize,
  uint? FlushTimer,
  DateTime? InstallDate,
  uint? InstalledSize,
  uint? LastErrorCode,
  ushort? Level,
  uint? LineSize,
  string? Location,
  uint? MaxCacheSize,
  string? Name,
  ulong? NumberOfBlocks,
  string? OtherErrorDescription,
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Purpose,
  ushort? ReadPolicy,
  ushort? ReplacementPolicy,
  ulong? StartingAddress,
  string? Status,
  ushort? StatusInfo,
  ushort[]? SupportedSRAM,
  string? SystemCreationClassName,
  bool? SystemLevelAddress,
  string? SystemName,
  ushort? WritePolicy) {
  public double? InstalledSizeInMB => InstalledSize is null ? null : Math.Round(InstalledSize.Value / 1024d, 2);
  public double? MaxCacheSizeInMB => MaxCacheSize is null ? null : Math.Round(MaxCacheSize.Value / 1024d, 2);
  public string? CacheTypeName => CacheType switch { 1 => "Other", 2 => "Unknown", 3 => "Instruction", 4 => "Data", 5 => "Unified", _ => null };
  public string? LevelName => Level switch { 3 => "L1", 4 => "L2", 5 => "L3", _ => null };
  public string? AssociativityName => Associativity switch { 1 => "Other", 2 => "Unknown", 3 => "Direct Mapped", 4 => "2-way Set-Associative", 5 => "4-way Set-Associative", 6 => "Fully Associative", 7 => "8-way Set-Associative", 8 => "16-way Set-Associative", _ => null };
}
