namespace Crystal.Provider.Mmi.HardwareFeatures.BaseBoard;

/// <summary>
/// Represents the static hardware metrics of a motherboard / baseboard. 
/// This is a record type, so it is immutable and supports value-based equality.
/// </summary>
/// <param name="Caption">The caption of the baseboard.</param>
/// <param name="CreationClassName">The class name of the creation.</param>
/// <param name="Description">The description of the baseboard.</param>
/// <param name="HostingBoard">Indicates whether the baseboard explicitly hosts main processor slots.</param>
/// <param name="HotSwappable">Indicates whether the baseboard is hot-swappable.</param>
/// <param name="InstallationDate">The installation date of the baseboard.</param>
/// <param name="Manufacturer">The manufacturer of the baseboard.</param>
/// <param name="Model">The model of the baseboard.</param>
/// <param name="Name">The name of the baseboard.</param>
/// <param name="PartNumber">The part number of the baseboard.</param>
/// <param name="Removable">Indicates whether the baseboard is removable.</param>
/// <param name="Replaceable">Indicates whether the baseboard is replaceable.</param>
/// <param name="Requirements">The requirements for the baseboard.</param>
/// <param name="SerialNumber">The serial number of the baseboard.</param>
/// <param name="SKU">The stock keeping unit of the baseboard.</param>
/// <param name="SlotLayout">The layout of the slots on the baseboard.</param>
/// <param name="SpecialRequirements">The special requirements for the baseboard.</param>
/// <param name="Status">The status of the baseboard.</param>
/// <param name="Tag">The tag for the baseboard.</param>
/// <param name="Version">The version of the baseboard.</param>
/// <param name="Weight">The weight of the baseboard.</param>
/// <param name="Width">The width of the baseboard.</param>
public record BaseBoardMetrics(
  string? Caption,
  string? CreationClassName,
  string? Description,
  bool? HostingBoard,       // True if this motherboard explicitly hosts main processor slots
  bool? HotSwappable,
  string? InstallationDate, // WMI often maps motherboard install dates directly as strings
  string? Manufacturer,     // e.g., "ASUSTeK COMPUTER INC.", "MSI"
  string? Model,
  string? Name,
  string? PartNumber,
  bool? Removable,
  bool? Replaceable,
  string? Requirements,
  string? SerialNumber,     // Motherboard unique tracking serial string
  string? SKU,
  string? SlotLayout,
  string? SpecialRequirements,
  string? Status,
  string? Tag,              // Unique identification key tag (e.g., "Base Board")
  string? Version,          // Motherboard hardware revision version
  float? Weight,
  float? Width
);
