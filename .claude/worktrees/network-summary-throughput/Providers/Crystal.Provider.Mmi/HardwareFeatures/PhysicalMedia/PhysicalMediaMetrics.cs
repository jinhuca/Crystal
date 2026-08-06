namespace Crystal.Provider.Mmi.HardwareFeatures.PhysicalMedia;

// Win32_PhysicalMedia is derived from CIM_PhysicalComponent (not CIM_LogicalDevice), so it
// has no DeviceID/PNPDeviceID/Availability/PowerManagement* trio seen on hardware feature
// classes — instead it identifies physical media by Tag (e.g. "\\.\PHYSICALDRIVE0").
public record PhysicalMediaMetrics(
  ulong? Capacity,
  string? Caption,
  bool? CleanerMedia,
  string? CreationClassName,
  string? Description,
  bool? HotSwappable,
  DateTime? InstallDate,
  string? Manufacturer,
  string? MediaDescription,
  ushort? MediaType,
  string? Model,
  string? Name,
  string? OtherIdentifyingInfo,
  string? PartNumber,
  bool? PoweredOn,
  bool? Removable,
  bool? Replaceable,
  string? SerialNumber,
  string? SKU,
  string? Status,
  string? Tag,           // e.g. "\\.\PHYSICALDRIVE0" — the key property
  string? Version,
  bool? WriteProtectOn
);
