namespace Crystal.Mmi.SoftwareFeatures.OSRecoveryConfiguration;

// Win32_OSRecoveryConfiguration is derived from CIM_Setting (not CIM_LogicalDevice), so it
// carries Caption/Description/SettingID rather than the usual DeviceID/Status/InstallDate
// trio seen on hardware feature classes. It describes what happens when the OS fails
// (boot failures and system crashes) — normally exactly one instance per system.
public record OSRecoveryConfigurationMetrics(
  bool? AutoReboot,
  string? Caption,
  string? DebugFilePath,
  uint? DebugInfoType,
  string? Description,
  string? ExpandedDebugFilePath,
  string? ExpandedMiniDumpDirectory,
  bool? KernelDumpOnly,
  string? MiniDumpDirectory,
  string? Name,
  bool? OverwriteExistingDebugFile,
  bool? SendAdminAlert,
  string? SettingID,
  bool? WriteDebugInfo,
  bool? WriteToSystemLog
);
