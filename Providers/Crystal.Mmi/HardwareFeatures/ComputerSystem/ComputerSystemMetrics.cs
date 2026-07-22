namespace Crystal.Mmi.HardwareFeatures.ComputerSystem;
public record ComputerSystemMetrics(
  string? Name,
  string? Manufacturer,
  string? Model,
  string? SystemType,
  string? Domain,
  string? DNSHostName,
  string? UserName,
  string? PrimaryOwnerName,
  ulong? TotalPhysicalMemory,
  uint? NumberOfProcessors,
  uint? NumberOfLogicalProcessors,
  bool? HypervisorPresent,
  string? Status);
