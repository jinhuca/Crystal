using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DeviceBus;

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
