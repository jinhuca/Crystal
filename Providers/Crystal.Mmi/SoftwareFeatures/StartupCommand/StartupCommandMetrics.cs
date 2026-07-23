namespace Crystal.Mmi.SoftwareFeatures.StartupCommand;

// Win32_StartupCommand is derived from CIM_Setting (not CIM_LogicalDevice), so it
// carries Caption/Description/SettingID instead of the usual Status/InstallDate pair
// seen on hardware feature classes.
public record StartupCommandMetrics(
  string? Caption,
  string? Command,       // e.g. "C:\Windows\notepad.exe myfile.txt"
  string? Description,
  string? Location,       // e.g. "Startup", "Common Startup", or a registry Run key path
  string? Name,
  string? SettingID,
  string? User,
  string? UserSID
);
