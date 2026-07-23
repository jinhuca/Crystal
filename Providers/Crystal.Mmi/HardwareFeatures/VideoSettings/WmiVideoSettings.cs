using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.VideoSettings;

internal static class WmiVideoSettings {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.VideoSettings;

  // ---------------------------------------------------------------------
  // Association Reference Properties
  // ---------------------------------------------------------------------
  public const string Element = nameof(Element);
  public const string Setting = nameof(Setting);
}
