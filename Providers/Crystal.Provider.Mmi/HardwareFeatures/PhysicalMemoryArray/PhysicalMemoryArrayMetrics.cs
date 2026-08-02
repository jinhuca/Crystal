namespace Crystal.Provider.Mmi.HardwareFeatures.PhysicalMemoryArray;

public record PhysicalMemoryArrayMetrics(
    ushort? Attributes, string? Caption, string? CreationClassName, ushort? Depth, string? Description,
    ushort? Height, bool? HotSwappable, DateTime? InstallDate, ushort? Location, string? Manufacturer,
    uint? MaxCapacity, ulong? MaxCapacityEx, ushort? MemoryDevices, ushort? MemoryErrorCorrection,
    string? Model, string? Name, string? OtherIdentifyingInfo, string? PartNumber, bool? PoweredOn,
    bool? Removable, bool? Replaceable, string? SerialNumber, string? SKU, string? Status,
    string? Tag, ushort? Use, string? Version, float? Weight, float? Width)
{
    public double? MaxCapacityInGB => MaxCapacity is null ? null : Math.Round(MaxCapacity.Value / 1024d / 1024d, 2);
    public double? MaxCapacityExInGB => MaxCapacityEx is null ? null : Math.Round(MaxCapacityEx.Value / 1024d / 1024d / 1024d, 2);
    public string? LocationName => Location switch { 1 => "Other", 2 => "Unknown", 3 => "System Board or Motherboard", 4 => "ISA Add-on Card", 5 => "EISA Add-on Card", 6 => "PCI Add-on Card", 7 => "MCA Add-on Card", 8 => "PCMCIA Add-on Card", 9 => "Proprietary Add-on Card", 10 => "NuBus", _ => null };
    public string? UseName => Use switch { 1 => "Other", 2 => "Unknown", 3 => "System Memory", 4 => "Video Memory", 5 => "Flash Memory", 6 => "Non-volatile RAM", 7 => "Cache Memory", _ => null };
    public string? MemoryErrorCorrectionName => MemoryErrorCorrection switch { 1 => "Other", 2 => "Unknown", 3 => "None", 4 => "Parity", 5 => "Single-bit ECC", 6 => "Multi-bit ECC", 7 => "CRC", _ => null };
}
