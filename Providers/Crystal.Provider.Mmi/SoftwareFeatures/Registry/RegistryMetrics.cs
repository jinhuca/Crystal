namespace Crystal.Provider.Mmi.SoftwareFeatures.Registry;

public record RegistryMetrics(
  string? Caption,
  uint? CurrentSize,     // megabytes
  string? Description,
  DateTime? InstallDate,
  uint? MaximumSize,     // megabytes; a proposed maximum that takes effect after reboot
  string? Name,
  uint? ProposedSize,    // megabytes
  string? Status
);
