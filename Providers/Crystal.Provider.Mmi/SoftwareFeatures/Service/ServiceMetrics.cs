namespace Crystal.Provider.Mmi.SoftwareFeatures.Service;

public record ServiceMetrics(
  bool? AcceptPause,
  bool? AcceptStop,
  string? Caption,
  string? CreationClassName,
  string? Description,
  bool? DesktopInteract,
  string? DisplayName,         // The user-facing service name (e.g., "Windows Update")
  string? ErrorControl,
  uint? ExitCode,
  DateTime? InstallDate,
  string? Name,                // The internal service registry key name (e.g., "wuauserv")
  string? PathName,            // Full execution path string (e.g., "C:\Windows\system32\svchost.exe -k netsvcs")
  uint? ProcessId,             // Active Process ID (PID) if the service is currently running
  uint? ServiceSpecificExitCode,
  string? ServiceType,
  bool? Started,               // True if the service is actively running
  string? StartMode,           // "Automatic", "Manual", "Disabled"
  string? StartName,           // The account context running the service (e.g., "LocalSystem", "NT AUTHORITY\LocalService")
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  uint? TagId,
  string? State                // "Running", "Stopped", "Start Pending", "Paused"
);
