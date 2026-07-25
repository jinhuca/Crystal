using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.IDEControllerDevice;

internal static class WmiIDEControllerDevice {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.IDEControllerDevice;

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
