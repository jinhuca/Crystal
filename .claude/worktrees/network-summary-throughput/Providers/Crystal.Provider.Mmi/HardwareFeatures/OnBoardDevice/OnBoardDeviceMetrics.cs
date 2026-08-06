namespace Crystal.Provider.Mmi.HardwareFeatures.OnBoardDevice;

// Win32_OnBoardDevice is derived from CIM_PhysicalComponent (not CIM_LogicalDevice), so it
// has no DeviceID/PNPDeviceID/Availability/PowerManagement* trio seen on hardware feature
// classes — instead it identifies onboard adapters (video, audio, NIC, SCSI) by Tag.
public record OnBoardDeviceMetrics(
  string? Caption,
  string? CreationClassName,
  string? Description,
  ushort? DeviceType,        // e.g., video, audio, network, SCSI
  bool? Enabled,
  bool? HotSwappable,
  DateTime? InstallDate,
  string? Manufacturer,
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
  string? Tag,
  string? Version
);
