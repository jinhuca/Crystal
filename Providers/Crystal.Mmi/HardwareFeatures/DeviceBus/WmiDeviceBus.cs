using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.DeviceBus;

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
