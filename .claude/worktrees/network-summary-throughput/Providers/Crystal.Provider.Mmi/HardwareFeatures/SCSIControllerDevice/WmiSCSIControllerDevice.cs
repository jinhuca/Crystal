using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.SCSIControllerDevice;

internal static class WmiSCSIControllerDevice {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.SCSIControllerDevice;

  // ---------------------------------------------------------------------
  // Association Reference / Telemetry Properties
  // ---------------------------------------------------------------------
  public const string AccessState = nameof(AccessState);
  public const string Antecedent = nameof(Antecedent);
  public const string Dependent = nameof(Dependent);
  public const string NegotiatedDataWidth = nameof(NegotiatedDataWidth);
  public const string NegotiatedSpeed = nameof(NegotiatedSpeed);
  public const string NumberOfHardResets = nameof(NumberOfHardResets);
  public const string NumberOfSoftResets = nameof(NumberOfSoftResets);
}
