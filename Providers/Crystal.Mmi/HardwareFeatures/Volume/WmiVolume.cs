using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.Volume;

internal static class WmiVolume
{
    public const string ClassName = WmiClasses.Volume;
    public const string Name = CommonWmiProperties.Name;
    public const string Caption = CommonWmiProperties.Caption;
    public const string Description = CommonWmiProperties.Description;
    public const string DeviceID = CommonWmiProperties.DeviceId;
    public const string Status = CommonWmiProperties.Status;
    public const string Automount = nameof(Automount);
    public const string Availability = nameof(Availability);
    public const string BlockSize = nameof(BlockSize);
    public const string BootVolume = nameof(BootVolume);
    public const string Capacity = nameof(Capacity);
    public const string Compressed = nameof(Compressed);
    public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
    public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
    public const string CreationClassName = nameof(CreationClassName);
    public const string DirtyBitSet = nameof(DirtyBitSet);
    public const string DriveLetter = nameof(DriveLetter);
    public const string DriveType = nameof(DriveType);
    public const string ErrorCleared = nameof(ErrorCleared);
    public const string ErrorDescription = nameof(ErrorDescription);
    public const string ErrorMethodology = nameof(ErrorMethodology);
    public const string FileSystem = nameof(FileSystem);
    public const string FreeSpace = nameof(FreeSpace);
    public const string IndexingEnabled = nameof(IndexingEnabled);
    public const string InstallationDate = nameof(InstallationDate);
    public const string Label = nameof(Label);
    public const string LastErrorCode = nameof(LastErrorCode);
    public const string MaximumFileNameLength = nameof(MaximumFileNameLength);
    public const string PageFilePresent = nameof(PageFilePresent);
    public const string PNPDeviceID = nameof(PNPDeviceID);
    public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
    public const string PowerManagementSupported = nameof(PowerManagementSupported);
    public const string Purpose = nameof(Purpose);
    public const string QuotasEnabled = nameof(QuotasEnabled);
    public const string QuotasIncomplete = nameof(QuotasIncomplete);
    public const string QuotasRebuilding = nameof(QuotasRebuilding);
    public const string SerialNumber = nameof(SerialNumber);
    public const string StatusInfo = nameof(StatusInfo);
    public const string SupportsDiskQuotas = nameof(SupportsDiskQuotas);
    public const string SupportsFileBasedCompression = nameof(SupportsFileBasedCompression);
    public const string SystemCreationClassName = nameof(SystemCreationClassName);
    public const string SystemName = nameof(SystemName);
    public const string SystemVolume = nameof(SystemVolume);
}
