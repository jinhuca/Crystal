namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystem;

/// <summary>
/// Represents the metrics of a computer system, including its name, manufacturer, model, system type, domain, 
/// DNS host name, user name, primary owner name, total physical memory, number of processors, number of 
/// logical processors, hypervisor presence, and status.
/// </summary>
/// <param name="Name">The name of the computer system.</param>
/// <param name="Manufacturer">The manufacturer of the computer system.</param>
/// <param name="Model">The model of the computer system.</param>
/// <param name="SystemType">The type of the computer system.</param>
/// <param name="Domain">The domain to which the computer system belongs.</param>
/// <param name="DNSHostName">The DNS host name of the computer system.</param>
/// <param name="UserName">The user name of the computer system.</param>
/// <param name="PrimaryOwnerName">The primary owner name of the computer system.</param>
/// <param name="TotalPhysicalMemory">The total physical memory of the computer system.</param>
/// <param name="NumberOfProcessors">The number of processors in the computer system.</param>
/// <param name="NumberOfLogicalProcessors">The number of logical processors in the computer system.</param>
/// <param name="HypervisorPresent">Indicates whether a hypervisor is present in the computer system.</param>
/// <param name="Status">The status of the computer system.</param>
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
