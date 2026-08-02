namespace Crystal.Provider.Mmi.SoftwareFeatures.LogonSession;

public record LogonSessionMetrics(
  string? AuthenticationPackage,
  string? Caption,
  string? Description,
  DateTime? InstallDate,
  string? LogonId,
  uint? LogonType,
  string? Name,
  DateTime? StartTime,
  string? Status
);
