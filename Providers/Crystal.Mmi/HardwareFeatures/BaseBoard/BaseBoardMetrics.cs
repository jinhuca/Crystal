namespace Crystal.Mmi.HardwareFeatures.BaseBoard;
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
