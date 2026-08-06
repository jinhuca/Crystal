using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.OnBoardDevice;

internal static class WmiOnBoardDevice {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.OnBoardDevice;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string CreationClassName = CommonWmiProperties.CreationClassName;
  public const string InstallDate = CommonWmiProperties.InstallDate;

  // ---------------------------------------------------------------------
  // OnBoard Device Specific Properties
  // ---------------------------------------------------------------------
  public const string DeviceType = nameof(DeviceType);
  public const string Enabled = nameof(Enabled);
  public const string HotSwappable = nameof(HotSwappable);
  public const string Model = nameof(Model);
  public const string OtherIdentifyingInfo = nameof(OtherIdentifyingInfo);
  public const string PartNumber = nameof(PartNumber);
  public const string PoweredOn = nameof(PoweredOn);
  public const string Removable = nameof(Removable);
  public const string Replaceable = nameof(Replaceable);
  public const string SerialNumber = nameof(SerialNumber);
  public const string SKU = nameof(SKU);
  public const string Tag = nameof(Tag);
  public const string Version = nameof(Version);
}
