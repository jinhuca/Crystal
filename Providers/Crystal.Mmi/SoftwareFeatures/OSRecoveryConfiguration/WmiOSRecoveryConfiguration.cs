using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.OSRecoveryConfiguration;

internal static class WmiOSRecoveryConfiguration {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.OSRecoveryConfiguration;

  // ---------------------------------------------------------------------
  // Shared Properties (CIM_Setting)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;

  // ---------------------------------------------------------------------
  // OS Recovery Configuration Specific Properties
  // ---------------------------------------------------------------------
  public const string AutoReboot = nameof(AutoReboot);
  public const string DebugFilePath = nameof(DebugFilePath);
  public const string DebugInfoType = nameof(DebugInfoType);
  public const string ExpandedDebugFilePath = nameof(ExpandedDebugFilePath);
  public const string ExpandedMiniDumpDirectory = nameof(ExpandedMiniDumpDirectory);
  public const string KernelDumpOnly = nameof(KernelDumpOnly);
  public const string MiniDumpDirectory = nameof(MiniDumpDirectory);
  public const string OverwriteExistingDebugFile = nameof(OverwriteExistingDebugFile);
  public const string SendAdminAlert = nameof(SendAdminAlert);
  public const string SettingID = nameof(SettingID);
  public const string WriteDebugInfo = nameof(WriteDebugInfo);
  public const string WriteToSystemLog = nameof(WriteToSystemLog);
}
