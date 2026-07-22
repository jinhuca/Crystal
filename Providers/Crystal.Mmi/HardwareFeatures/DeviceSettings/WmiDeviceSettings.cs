using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.DeviceSettings;

internal static class WmiDeviceSettings {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.DeviceSettings;

  // ---------------------------------------------------------------------
  // Association Reference Properties
  // ---------------------------------------------------------------------
  public const string Element = nameof(Element);
  public const string Setting = nameof(Setting);
}
