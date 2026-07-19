using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class MemoryConstants {
  public const string QueryString = "SELECT * FROM Win32_PhysicalMemory";

  public const string AttributesKey = "Attributes";
  public const string AttributesDesc = "Bitmask describing the memory module's compatibility with EDB-338 hot-swappable slots";

  public const string BankLabelKey = "BankLabel";
  public const string BankLabelDesc = "Label of the physically labeled bank where the memory module is located";

  public const string CapacityKey = "Capacity";
  public const string CapacityDesc = "Total capacity of the physical memory module, in bytes";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the physical memory module";

  public const string ConfiguredClockSpeedKey = "ConfiguredClockSpeed";
  public const string ConfiguredClockSpeedDesc = "Configured clock speed of the memory module, in MHz";

  public const string ConfiguredVoltageKey = "ConfiguredVoltage";
  public const string ConfiguredVoltageDesc = "Configured voltage for the memory module, in millivolts";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Name of the first concrete class to appear in the inheritance chain";

  public const string DataWidthKey = "DataWidth";
  public const string DataWidthDesc = "Data width, in bits, of the physical memory module";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the physical memory module";

  public const string DeviceLocatorKey = "DeviceLocator";
  public const string DeviceLocatorDesc = "Physically-labeled socket or circuit board label for the memory module";

  public const string FormFactorKey = "FormFactor";
  public const string FormFactorDesc = "Implementation form factor for the memory module (DIMM, SODIMM, etc.)";

  public const string HotSwappableKey = "HotSwappable";
  public const string HotSwappableDesc = "If True, the memory module can be replaced while the system is running";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the memory module was installed";

  public const string InterleaveDataDepthKey = "InterleaveDataDepth";
  public const string InterleaveDataDepthDesc = "Maximum number of consecutive rows on the physical memory module that can be accessed without waiting";

  public const string InterleavePositionKey = "InterleavePosition";
  public const string InterleavePositionDesc = "Position of the memory module in an interleave (0 if not interleaved)";

  public const string ManufacturerKey = "Manufacturer";
  public const string ManufacturerDesc = "Name of the memory module's manufacturer";

  public const string MaxVoltageKey = "MaxVoltage";
  public const string MaxVoltageDesc = "Maximum voltage for the memory module, in millivolts";

  public const string MemoryTypeKey = "MemoryType";
  public const string MemoryTypeDesc = "Type of physical memory (DDR, DDR4, etc.)";

  public const string MinVoltageKey = "MinVoltage";
  public const string MinVoltageDesc = "Minimum voltage for the memory module, in millivolts";

  public const string ModelKey = "Model";
  public const string ModelDesc = "Model number of the memory module, set by the manufacturer";

  public const string NameKey = "Name";
  public const string NameDesc = "Label by which the physical memory module is known";

  public const string OtherIdentifyingInfoKey = "OtherIdentifyingInfo";
  public const string OtherIdentifyingInfoDesc = "Additional data, in addition to asset tag/serial number, used to identify the memory module";

  public const string PartNumberKey = "PartNumber";
  public const string PartNumberDesc = "Part number assigned by the manufacturer";

  public const string PositionInRowKey = "PositionInRow";
  public const string PositionInRowDesc = "Position of the memory module in a row (0 if not part of a row)";

  public const string PoweredOnKey = "PoweredOn";
  public const string PoweredOnDesc = "If True, the memory module is powered";

  public const string RemovableKey = "Removable";
  public const string RemovableDesc = "If True, the memory module can be removed from the physical container without impairment";

  public const string ReplaceableKey = "Replaceable";
  public const string ReplaceableDesc = "If True, the memory module can be replaced with one of an identical type";

  public const string SerialNumberKey = "SerialNumber";
  public const string SerialNumberDesc = "Serial number assigned by the manufacturer";

  public const string SKUKey = "SKU";
  public const string SKUDesc = "Manufacturer's stock keeping unit number for the memory module";

  public const string SMBIOSMemoryTypeKey = "SMBIOSMemoryType";
  public const string SMBIOSMemoryTypeDesc = "Type of physical memory as reported by SMBIOS, supersedes MemoryType";

  public const string SpeedKey = "Speed";
  public const string SpeedDesc = "Speed of the memory module, in nanoseconds or MHz depending on provider";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the memory module";

  public const string TagKey = "Tag";
  public const string TagDesc = "Identifier used to distinguish this memory module from other devices in the system";

  public const string TotalWidthKey = "TotalWidth";
  public const string TotalWidthDesc = "Total width, in bits, of the physical memory module, including check or error correction bits";

  public const string TypeDetailKey = "TypeDetail";
  public const string TypeDetailDesc = "Additional detail on the memory module's physical type";

  public const string VersionKey = "Version";
  public const string VersionDesc = "Version of the physical memory module, set by the manufacturer";

  public const string CapacityUnit = "bytes";
  public const string SpeedUnit = "MHz";
}
