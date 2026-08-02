using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.HardwareFeatures.Tpm;

internal static class WmiTpm {
  // ---------------------------------------------------------------------
  // WMI Class + Namespace
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.Tpm;
  public const string Namespace = @"root\cimv2\Security\MicrosoftTpm";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // TPM Specific Properties
  // ---------------------------------------------------------------------
  public const string InstanceName = nameof(InstanceName);
  public const string IsActivated_InitialValue = nameof(IsActivated_InitialValue);
  public const string IsEnabled_InitialValue = nameof(IsEnabled_InitialValue);
  public const string IsOwned_InitialValue = nameof(IsOwned_InitialValue);
  public const string ManufacturerId = nameof(ManufacturerId);
  public const string ManufacturerIdTxt = nameof(ManufacturerIdTxt);
  public const string ManufacturerVersion = nameof(ManufacturerVersion);
  public const string ManufacturerVersionFull20 = nameof(ManufacturerVersionFull20);
  public const string ManufacturerVersionInfo = nameof(ManufacturerVersionInfo);
  public const string PhysicalPresenceVersionInfo = nameof(PhysicalPresenceVersionInfo);
  public const string SpecVersion = nameof(SpecVersion);
}
