namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystemProduct;

/// <summary>
/// Represents the metrics of a computer system product, including its name, vendor, version, 
/// UUID, identifying number, SKU number, caption, and description.
/// </summary>
/// <param name="Name">The name of the computer system product.</param>
/// <param name="Vendor">The vendor of the computer system product.</param>
/// <param name="Version">The version of the computer system product.</param>
/// <param name="UUID">The UUID of the computer system product.</param>
/// <param name="IdentifyingNumber">The identifying number of the computer system product.</param>
/// <param name="SKUNumber">The SKU number of the computer system product.</param>
/// <param name="Caption">The caption of the computer system product.</param>
/// <param name="Description">The description of the computer system product.</param>
public record ComputerSystemProductMetrics(
  string? Name,
  string? Vendor,
  string? Version,
  string? UUID,
  string? IdentifyingNumber,
  string? SKUNumber,
  string? Caption,
  string? Description);
