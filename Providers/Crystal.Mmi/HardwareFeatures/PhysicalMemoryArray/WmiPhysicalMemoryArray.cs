using Crystal.Mmi.Wmi;
namespace Crystal.Mmi.HardwareFeatures.PhysicalMemoryArray;
internal static class WmiPhysicalMemoryArray
{
    public const string ClassName = WmiClasses.PhysicalMemoryArray;
    public const string Caption = CommonWmiProperties.Caption;
    public const string Description = CommonWmiProperties.Description;
    public const string Manufacturer = CommonWmiProperties.Manufacturer;
    public const string Name = CommonWmiProperties.Name;
    public const string Status = CommonWmiProperties.Status;
    public const string Attributes = nameof(Attributes);
    public const string CreationClassName = nameof(CreationClassName);
    public const string Depth = nameof(Depth);
    public const string Height = nameof(Height);
    public const string HotSwappable = nameof(HotSwappable);
    public const string InstallationDate = nameof(InstallationDate);
    public const string Location = nameof(Location);
    public const string MaxCapacity = nameof(MaxCapacity);
    public const string MaxCapacityEx = nameof(MaxCapacityEx);
    public const string MemoryDevices = nameof(MemoryDevices);
    public const string MemoryErrorCorrection = nameof(MemoryErrorCorrection);
    public const string Model = nameof(Model);
    public const string OtherIdentifyingInfo = nameof(OtherIdentifyingInfo);
    public const string PartNumber = nameof(PartNumber);
    public const string PoweredOn = nameof(PoweredOn);
    public const string Removable = nameof(Removable);
    public const string Replaceable = nameof(Replaceable);
    public const string SerialNumber = nameof(SerialNumber);
    public const string SKU = nameof(SKU);
    public const string Tag = nameof(Tag);
    public const string Use = nameof(Use);
    public const string Version = nameof(Version);
    public const string Weight = nameof(Weight);
    public const string Width = nameof(Width);
}
