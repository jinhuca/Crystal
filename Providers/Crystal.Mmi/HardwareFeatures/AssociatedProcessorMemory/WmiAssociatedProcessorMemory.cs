using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.AssociatedProcessorMemory;

internal static class WmiAssociatedProcessorMemory {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.AssociatedProcessorMemory;

  // ---------------------------------------------------------------------
  // Association Reference / Telemetry Properties
  // ---------------------------------------------------------------------
  public const string Antecedent = nameof(Antecedent);
  public const string BusSpeed = nameof(BusSpeed);
  public const string Dependent = nameof(Dependent);
}
