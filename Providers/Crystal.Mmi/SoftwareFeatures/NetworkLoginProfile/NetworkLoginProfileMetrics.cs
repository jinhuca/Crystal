namespace Crystal.Mmi.SoftwareFeatures.NetworkLoginProfile;

// Win32_NetworkLoginProfile is derived from CIM_Setting (not CIM_LogicalDevice), so it
// carries Caption/Description/SettingID instead of the usual DeviceID/Status/PNPDeviceID
// trio seen on hardware feature classes.
public record NetworkLoginProfileMetrics(
  DateTime? AccountExpires,
  uint? AuthorizationFlags,
  uint? BadPasswordCount,
  string? Caption,
  uint? CodePage,
  string? Comment,
  uint? CountryCode,
  string? Description,
  uint? Flags,
  string? FullName,
  string? HomeDirectory,
  string? HomeDirectoryDrive,
  DateTime? LastLogoff,
  DateTime? LastLogon,
  string? LogonHours,
  string? LogonServer,
  ulong? MaximumStorage,
  string? Name,
  uint? NumberOfLogons,
  string? Parameters,
  DateTime? PasswordAge,       // WMI datetime "interval" format — elapsed time, not an absolute date
  DateTime? PasswordExpires,
  uint? PrimaryGroupId,
  uint? Privileges,
  string? Profile,
  string? ScriptPath,
  string? SettingID,
  uint? UnitsPerWeek,
  string? UserComment,
  uint? UserId,
  string? UserType,
  string? Workstations
);
