namespace Crystal.Mmi.HardwareFeatures.PowerManagementEvent;

// Win32_PowerManagementEvent is an extrinsic WMI *event* class (derived from
// __ExtrinsicEvent), not an ordinary polled data class. In real WMI it's normally
// consumed via an event subscription (e.g. a temporary event query/watcher) that
// fires when a power state change occurs — not via a one-shot SELECT * enumeration.
// It's included here for API completeness/consistency with the rest of the feature
// set, but ToSafePowerManagementEventMetricsAsync will typically return an empty
// list when queried through the same instance-enumeration path used elsewhere,
// since there's no "current" event to enumerate outside of an active subscription.
public record PowerManagementEventMetrics(
  ushort? EventType,     // 4 = Entering Suspend, 7 = Resume From Suspend, 10 = Power Status Change, 11 = OEM Event, 18 = Resume Automatic
  ushort? OEMEventCode   // only meaningful when EventType == 11 (OEM Event)
);
