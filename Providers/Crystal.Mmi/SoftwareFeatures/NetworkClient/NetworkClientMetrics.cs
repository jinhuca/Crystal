namespace Crystal.Mmi.SoftwareFeatures.NetworkClient;

public record NetworkClientMetrics(
  string? Caption,
  string? Description,
  DateTime? InstallDate,
  string? Manufacturer,
  string? Name,
  string? Status
);
