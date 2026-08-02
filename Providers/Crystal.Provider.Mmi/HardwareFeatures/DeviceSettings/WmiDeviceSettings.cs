using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.DeviceSettings;

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
