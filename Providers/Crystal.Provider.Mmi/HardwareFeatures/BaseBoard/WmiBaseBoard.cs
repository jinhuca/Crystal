using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.BaseBoard;

internal static class WmiBaseBoard {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.BaseBoard;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Manufacturer = CommonWmiProperties.Manufacturer;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // Base Board Specific Properties
  // ---------------------------------------------------------------------
  public const string ConfigOptions = nameof(ConfigOptions);
  public const string CreationClassName = nameof(CreationClassName);
  public const string Depth = nameof(Depth);
  public const string Height = nameof(Height);
  public const string HostingBoard = nameof(HostingBoard);
  public const string HotSwappable = nameof(HotSwappable);
  public const string InstallationDate = nameof(InstallationDate);
  public const string Model = nameof(Model);
  public const string Name = nameof(Name);
  public const string PartNumber = nameof(PartNumber);
  public const string PoweredOn = nameof(PoweredOn);
  public const string Product = nameof(Product);
  public const string Removable = nameof(Removable);
  public const string Replaceable = nameof(Replaceable);
  public const string RequirementsDescription = nameof(RequirementsDescription);
  public const string SerialNumber = nameof(SerialNumber);
  public const string SKU = nameof(SKU);
  public const string SlotLayout = nameof(SlotLayout);
  public const string SpecialRequirements = nameof(SpecialRequirements);
  public const string Tag = nameof(Tag);
  public const string Version = nameof(Version);
  public const string Weight = nameof(Weight);
  public const string Width = nameof(Width);
}