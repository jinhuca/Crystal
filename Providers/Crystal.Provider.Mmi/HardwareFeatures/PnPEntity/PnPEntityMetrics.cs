namespace Crystal.Provider.Mmi.HardwareFeatures.PnPEntity;

public record PnPEntityMetrics(
  ushort? Availability,
  string? Caption,
  string? ClassGuid,
  string? CompatibleID,        // Maps from string[] array types (Flattened)
  uint? ConfigManagerErrorCode, // 0 = Working correctly, 22 = Disabled, etc.
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  string? Description,
  string? DeviceID,            // Key unique hardware instance ID path
  bool? ErrorCleared,
  string? ErrorDescription,
  string? HardwareID,          // Maps from string[] array types (Flattened)
  DateTime? InstallDate,
  uint? LastErrorCode,
  string? Manufacturer,
  string? Name,                // Device name as seen in Device Manager
  string? PNPClass,            // The Device Category (e.g., "Keyboard", "Mouse", "USB")
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  bool? Present,               // True if the device is currently physically connected
  string? Service,             // The name of the kernel driver supporting the device
  string? Status,              // e.g., "OK", "Error", "Degraded"
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
);
