namespace Crystal.Provider.Mmi.HardwareFeatures.ComputerSystemProduct;
public record ComputerSystemProductMetrics(
  string? Name,
  string? Vendor,
  string? Version,
  string? UUID,
  string? IdentifyingNumber,
  string? SKUNumber,
  string? Caption,
  string? Description);
