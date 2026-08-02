using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.PowerManagementEvent;

internal static class WmiPowerManagementEvent {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.PowerManagementEvent;

  // ---------------------------------------------------------------------
  // Power Management Event Properties
  // (This is an extrinsic event class — see remarks in PowerManagementEventMetrics.cs.)
  // ---------------------------------------------------------------------
  public const string EventType = nameof(EventType);
  public const string OEMEventCode = nameof(OEMEventCode);
}
