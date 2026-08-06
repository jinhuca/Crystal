using Crystal.Provider.Mmi.Wmi;
namespace Crystal.Provider.Mmi.HardwareFeatures.SystemSlot;
internal static class WmiSystemSlot
{
    public const string ClassName = WmiClasses.SystemSlot;
    public const string Caption = CommonWmiProperties.Caption;
    public const string Description = CommonWmiProperties.Description;
    public const string Manufacturer = CommonWmiProperties.Manufacturer;
    public const string Name = CommonWmiProperties.Name;
    public const string Status = CommonWmiProperties.Status;
    public const string ConnectorPinout = nameof(ConnectorPinout); public const string ConnectorType = nameof(ConnectorType); public const string CreationClassName = nameof(CreationClassName); public const string CurrentUsage = nameof(CurrentUsage); public const string HeightAllowed = nameof(HeightAllowed); public const string InstallationDate = nameof(InstallationDate); public const string LengthAllowed = nameof(LengthAllowed); public const string MaxDataWidth = nameof(MaxDataWidth); public const string Model = nameof(Model); public const string Number = nameof(Number); public const string OtherIdentifyingInfo = nameof(OtherIdentifyingInfo); public const string PartNumber = nameof(PartNumber); public const string PMESignal = nameof(PMESignal); public const string PoweredOn = nameof(PoweredOn); public const string PurposeDescription = nameof(PurposeDescription); public const string SerialNumber = nameof(SerialNumber); public const string Shared = nameof(Shared); public const string SKU = nameof(SKU); public const string SlotDesignation = nameof(SlotDesignation); public const string SpecialPurpose = nameof(SpecialPurpose); public const string SupportsHotPlug = nameof(SupportsHotPlug); public const string Tag = nameof(Tag); public const string ThermalRating = nameof(ThermalRating); public const string VccMixedVoltageSupport = nameof(VccMixedVoltageSupport); public const string Version = nameof(Version); public const string VppMixedVoltageSupport = nameof(VppMixedVoltageSupport);
}
