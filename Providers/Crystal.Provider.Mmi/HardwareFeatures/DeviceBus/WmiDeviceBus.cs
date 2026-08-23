using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DeviceBus;

/// <summary>
/// Contains the WMI class and property names for the DeviceBus feature. 
/// This class is used to interact with WMI to retrieve information about 
/// device bus associations in the system.
/// </summary>
internal static class WmiDeviceBus {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.DeviceBus;

  // ---------------------------------------------------------------------
  // Association Reference Properties
  // ---------------------------------------------------------------------
  public const string Antecedent = nameof(Antecedent);
  public const string Dependent = nameof(Dependent);
}
