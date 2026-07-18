using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class CpuConstants {
  public const string QueryString = "SELECT * FROM Win32_Processor";

  public const string AddressWidthKey = "AddressWidth";
  public const string AddressWidthDesc = "Processor Address Width";

  public const string ArchitectureKey = "Architecture";
  public const string ArchitectureDesc = "Processor Architecture";

  public const string AssetTagKey = "AssetTag";
  public const string AssetTagDesc = "Asset Tag Identifier";

  public const string AvailabilityKey = "Availability";
  public const string AvailabilityDesc = "";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Prcessor Caption";

  public const string CharacteristicsKey = "Characteristics";
  public const string CharacteristicsDesc = "Processor Functionality";

  public const string ConfigManagerErrorCodeKey = "ConfigManagerErrorCode";
  public const string ConfigManagerErrorCodeDesc = "Configuration Manager Error Code";

  public const string ConfigManagerUserConfigKey = "ConfigManagerUserConfig";
  public const string ConfigManagerUserConfigDesc = "Processor Configured (True/False)";

  public const string CpuStatusKey = "CpuStatus";
  public const string CpuStatusDesc = "Current status of the processor";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Creation Class Name";

  public const string CurrentClockSpeedKey = "CurrentClockSpeed";
  public const string CurrentClockSpeedDesc = "Processor Current Clock Speed";

  public const string CurrentVoltageKey = "CurrentVoltage";
  public const string CurrentVoltageDesc = "Voltage of the processor";

  public const string DataWidthKey = "DataWidth";
  public const string DataWidthDesc = "Processor Data Width";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Processor Description";

  public const string DeviceIDKey = "DeviceID";
  public const string DeviceIDDesc = "Unique identifier of a processor on the system";

  public const string ErrorClearedKey = "ErrorCleared";
  public const string ErrorClearedDesc = "If True, the error reported in LastErrorCode is clear";

  public const string ErrorDescriptionKey = "ErrorDescription";
  public const string ErrorDescriptionDesc = "Information about the error recorded in LastErrorCode";

  public const string ExtClockKey = "ExtClock";
  public const string ExtClockDesc = "External clock frequency in MHz";

  public const string FamilyKey = "Family";
  public const string FamilyDesc = "Processor Family";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the object is installed";

  public const string L2CacheSizeKey = "L2CacheSize";
  public const string L2CacheSizeDesc = "Size of the Level 2 processor cache (KB)";

  public const string L2CacheSpeedKey = "L2CacheSpeed";
  public const string L2CacheSpeedDesc = "Clock speed of the Level 2 processor cache (MHz)";

  public const string L3CacheSizeKey = "L3CacheSize";
  public const string L3CacheSizeDesc = "Size of the Level 3 processor cache (KB)";

  public const string L3CacheSpeedKey = "L3CacheSpeed";
  public const string L3CacheSpeedDesc = "Clock speed of the Level 3 processor cache (MHz)";

  public const string LastErrorCodeKey = "LastErrorCode";
  public const string LastErrorCodeDesc = "Last error code reported by processor.";

  public const string LevelKey = "Level";
  public const string LevelDesc = "Processor Type Level";

  public const string LoadPercentageKey = "LoadPercentage";
  public const string LoadPercentageDesc = "Load capacity of each processor, averaged to the last second";

  public const string NameKey = "Name";
  public const string NameDesc = "Processor Name";

  public const string ManufacturerKey = "Manufacturer";
  public const string ManufacturerDesc = "Name of the processor manufacturer";

  public const string MaxClockSpeedKey = "MaxClockSpeed";
  public const string MaxClockSpeedDesc = "Max Clock Speed";

  public const string NumberOfCoresKey = "NumberOfCores";
  public const string NumberOfCoresDesc = "Number of cores for the current instance of the processor";

  public const string NumberOfEnabledCoreKey = "NumberOfEnabledCore";
  public const string NumberOfEnabledCoreDesc = "The number of enabled cores per processor socket";

  public const string NumberOfLogicalProcessorsKey = "NumberOfLogicalProcessors";
  public const string NumberOfLogicalProcessorsDesc = "Number of logical processors for the current instance of the processor";

  public const string OtherFamilyDescriptionKey = "OtherFamilyDescription";
  public const string OtherFamilyDescriptionDesc = "Processor family type";

  public const string PartNumberKey = "PartNumber";
  public const string PartNumberDesc = "The part number of this processor as set by the manufacturer";

  public const string PNPDeviceIDKey = "PNPDeviceID";
  public const string PNPDeviceIDDesc = "Windows Plug and Play device identifier of the logical device";

  // Not use.
  public const string PowerManagementCapabilitiesKey = "PowerManagementCapabilities";
  public const string PowerManagementCapabilitiesDesc = "Array of the specific power-related capabilities of a logical device";

  public const string ProcessorIdKey = "ProcessorId";
  public const string ProcessorIdDesc = "Processor ID";

  public const string ProcessorTypeKey = "ProcessorType";
  public const string ProcessorTypeDesc = "Primary function of the processor";

  public const string RevisionKey = "Revision";
  public const string RevisionDesc = "System revision level that depends on the architecture";

  public const string RoleKey = "Role";
  public const string RoleDesc = "Role of the processor";

  public const string SerialNumberKey = "SerialNumber";
  public const string SerialNumberDesc = "The serial number of this processor";

  public const string SocketDesignationKey = "SocketDesignation";
  public const string SocketDesignationDesc = "Type of chip socket used on the circuit";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the processor";

  public const string StatusInfoKey = "StatusInfo";
  public const string StatusInfoDesc = "State of the logical device";

  public const string SteppingKey = "Stepping";
  public const string SteppingDesc = "Revision level of the processor in the processor family";

  public const string SystemCreationClassNameKey = "SystemCreationClassName";
  public const string SystemCreationClassNameDesc = "Value of the CreationClassName property for the scoping computer";

  public const string SystemNameKey = "SystemName";
  public const string SystemNameDesc = "Name of the scoping system";

  public const string ThreadCountKey = "ThreadCount";
  public const string ThreadCountDesc = "The number of threads per processor socket";

  public const string UniqueIdKey = "UniqueId";
  public const string UniqueIdDesc = "Globally unique identifier for the processor";

  public const string UpgradeMethodKey = "UpgradeMethod";
  public const string UpgradeMethodDesc = "CPU socket information";

  public const string VersionKey = "Version";
  public const string VersionDesc = "Processor revision number that depends on the architecture";

  public const string VirtualizationFirmwareEnabledKey = "VirtualizationFirmwareEnabled";
  public const string VirtualizationFirmwareEnabledDesc = "If True, the Firmware has enabled virtualization extensions";

  public const string VMMonitorModeExtensionsKey = "VMMonitorModeExtensions";
  public const string VMMonitorModeExtensionsDesc = "If True, the processor supports Intel or AMD Virtual Machine Monitor extensions";

  public const string VoltageCapsKey = "VoltageCaps";
  public const string VoltageCapsDesc = "Voltage capabilities of the processor";

  public const string SpeedUnit = "MHz";
  public const string CacheSizeUnit = "KB";
}
