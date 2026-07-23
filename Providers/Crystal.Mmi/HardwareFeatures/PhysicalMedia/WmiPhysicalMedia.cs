using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.PhysicalMedia;

internal static class WmiPhysicalMedia {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.PhysicalMedia;

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
  // Physical Media Specific Properties
  // ---------------------------------------------------------------------
  public const string Capacity = nameof(Capacity);
  public const string CleanerMedia = nameof(CleanerMedia);
  public const string HotSwappable = nameof(HotSwappable);
  public const string MediaDescription = nameof(MediaDescription);
  public const string MediaType = nameof(MediaType);
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
  public const string WriteProtectOn = nameof(WriteProtectOn);
}
