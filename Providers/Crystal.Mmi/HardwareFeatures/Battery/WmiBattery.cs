using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.HardwareFeatures.Battery;

internal static class WmiBattery {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = "Win32_Battery";

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Status = CommonWmiProperties.Status;
  public const string Name = CommonWmiProperties.Name;
  public const string DeviceID = CommonWmiProperties.DeviceId;
  public const string PNPDeviceID = CommonWmiProperties.PnpDeviceId;

  // ---------------------------------------------------------------------
  // Battery Specific Properties
  // ---------------------------------------------------------------------
  public const string Availability = nameof(Availability);
  public const string BatteryRechargeTime = nameof(BatteryRechargeTime);
  public const string BatteryStatus = nameof(BatteryStatus);
  public const string Chemistry = nameof(Chemistry);
  public const string ConfigManagerErrorCode = nameof(ConfigManagerErrorCode);
  public const string ConfigManagerUserConfig = nameof(ConfigManagerUserConfig);
  public const string CreationClassName = nameof(CreationClassName);
  public const string DesignCapacity = nameof(DesignCapacity);
  public const string DesignVoltage = nameof(DesignVoltage);
  public const string ErrorCleared = nameof(ErrorCleared);
  public const string ErrorDescription = nameof(ErrorDescription);
  public const string EstimatedChargeRemaining = nameof(EstimatedChargeRemaining);
  public const string EstimatedRunTime = nameof(EstimatedRunTime);
  public const string ExpectedBatteryLife = nameof(ExpectedBatteryLife);
  public const string ExpectedLife = nameof(ExpectedLife);
  public const string FullChargeCapacity = nameof(FullChargeCapacity);
  public const string InstallationDate = nameof(InstallationDate);
  public const string LastErrorCode = nameof(LastErrorCode);
  public const string MaxRechargeTime = nameof(MaxRechargeTime);
  public const string PowerManagementCapabilities = nameof(PowerManagementCapabilities);
  public const string PowerManagementSupported = nameof(PowerManagementSupported);
  public const string SmartBatteryVersion = nameof(SmartBatteryVersion);
  public const string StatusInfo = nameof(StatusInfo);
  public const string SystemCreationClassName = nameof(SystemCreationClassName);
  public const string SystemName = nameof(SystemName);
  public const string TimeOnBattery = nameof(TimeOnBattery);
  public const string TimeToFullCharge = nameof(TimeToFullCharge);
}