using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystem;

/// <summary>
/// Contains the WMI class name and property names for <c>Win32_ComputerSystem</c>.
/// </summary>
internal static class WmiComputerSystem {
  public const string ClassName = WmiClasses.ComputerSystem;
  public const string Name = CommonWmiProperties.Name;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;
  public const string Model = nameof(Model);
  public const string SystemType = nameof(SystemType);
  public const string Domain = nameof(Domain);
  public const string DNSHostName = nameof(DNSHostName);
  public const string UserName = nameof(UserName);
  public const string PrimaryOwnerName = nameof(PrimaryOwnerName);
  public const string TotalPhysicalMemory = nameof(TotalPhysicalMemory);
  public const string NumberOfProcessors = nameof(NumberOfProcessors);
  public const string NumberOfLogicalProcessors = nameof(NumberOfLogicalProcessors);
  public const string HypervisorPresent = nameof(HypervisorPresent);
}