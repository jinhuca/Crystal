namespace Crystal.Mmi.SoftwareFeatures.Environment;

public record EnvironmentMetrics(
  string? Caption,
  string? Description,
  DateTime? InstallDate,
  string? Name,
  string? Status,
  bool? SystemVariable,
  string? UserName,     // e.g. "<System>" for system variables
  string? VariableValue
);
