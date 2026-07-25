using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.UserDesktop;

internal static class WmiUserDesktop {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.UserDesktop;

  // ---------------------------------------------------------------------
  // Association Reference Properties
  // ---------------------------------------------------------------------
  public const string Element = nameof(Element);
  public const string Setting = nameof(Setting);
}
