using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.AssociatedProcessorMemory;

/// <summary>
/// Contains the WMI class name and property names for the <c>AssociatedProcessorMemory</c> association.
/// </summary>
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
