namespace Crystal.Provider.Mmi.SoftwareFeatures.SystemDriver;

// Win32_SystemDriver is derived from Win32_BaseService, same as Win32_Service, but represents
// a kernel/system driver rather than a user-mode service (e.g. "Started" refers to the driver
// being loaded, and there is no ProcessId since drivers run in kernel space).
public record SystemDriverMetrics(
  bool? AcceptPause,
  bool? AcceptStop,
  string? Caption,
  string? CreationClassName,
  string? Description,
  bool? DesktopInteract,
  string? DisplayName,          // e.g., "WSL"
  string? ErrorControl,
  uint? ExitCode,
  DateTime? InstallDate,
  string? Name,                 // e.g., "lxss"
  string? PathName,             // e.g., "C:\\Windows\\system32\\drivers\\lxss.sys"
  uint? ServiceSpecificExitCode,
  string? ServiceType,
  bool? Started,
  string? StartMode,            // "Boot", "System", "Auto", "Manual", "Disabled"
  string? StartName,
  string? State,                // "Running", "Stopped", etc.
  string? Status,
  string? SystemCreationClassName,
  string? SystemName,
  uint? TagId
);
