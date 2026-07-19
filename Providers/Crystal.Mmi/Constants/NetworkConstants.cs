using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class NetworkConstants {
  public const string QueryString = "SELECT * FROM Win32_NetworkAdapter";

  public const string AdapterTypeKey = "AdapterType";
  public const string AdapterTypeDesc = "Network medium in use, e.g. Ethernet 802.3";

  public const string AdapterTypeIdKey = "AdapterTypeId";
  public const string AdapterTypeIdDesc = "Network medium in use, as an NDIS medium type value";

  public const string AutoSenseKey = "AutoSense";
  public const string AutoSenseDesc = "If True, the adapter can automatically determine the speed of the attached media";

  public const string AvailabilityKey = "Availability";
  public const string AvailabilityDesc = "Availability and status of the network adapter";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the network adapter";

  public const string ConfigManagerErrorCodeKey = "ConfigManagerErrorCode";
  public const string ConfigManagerErrorCodeDesc = "Windows Plug and Play error code";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Name of the first concrete class to appear in the inheritance chain";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the network adapter";

  public const string DeviceIDKey = "DeviceID";
  public const string DeviceIDDesc = "Identifier by which the network adapter is known";

  public const string GUIDKey = "GUID";
  public const string GUIDDesc = "Globally unique identifier for the network connection";

  public const string IndexKey = "Index";
  public const string IndexDesc = "Index number of the network adapter, stored as a string";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the network adapter was installed";

  public const string InstalledKey = "Installed";
  public const string InstalledDesc = "If True, the network adapter is installed";

  public const string InterfaceIndexKey = "InterfaceIndex";
  public const string InterfaceIndexDesc = "Index value that uniquely identifies the local network interface";

  public const string MACAddressKey = "MACAddress";
  public const string MACAddressDesc = "Media Access Control address of the network adapter";

  public const string ManufacturerKey = "Manufacturer";
  public const string ManufacturerDesc = "Name of the network adapter's manufacturer";

  public const string MaxSpeedKey = "MaxSpeed";
  public const string MaxSpeedDesc = "Maximum speed, in bits per second, for the network adapter";

  public const string NameKey = "Name";
  public const string NameDesc = "Label by which the network adapter is known";

  public const string NetConnectionIDKey = "NetConnectionID";
  public const string NetConnectionIDDesc = "Name of the network connection as it appears in Windows (e.g. 'Ethernet', 'Wi-Fi')";

  public const string NetConnectionStatusKey = "NetConnectionStatus";
  public const string NetConnectionStatusDesc = "State of the network adapter's connection to the network";

  public const string NetEnabledKey = "NetEnabled";
  public const string NetEnabledDesc = "If True, the network adapter is enabled";

  public const string PhysicalAdapterKey = "PhysicalAdapter";
  public const string PhysicalAdapterDesc = "If True, the adapter is a physical device rather than a logical/virtual one";

  public const string PNPDeviceIDKey = "PNPDeviceID";
  public const string PNPDeviceIDDesc = "Windows Plug and Play device identifier of the network adapter";

  public const string ProductNameKey = "ProductName";
  public const string ProductNameDesc = "Product name of the network adapter, as assigned by the manufacturer";

  public const string ServiceNameKey = "ServiceName";
  public const string ServiceNameDesc = "Service name of the network adapter's driver";

  public const string SpeedKey = "Speed";
  public const string SpeedDesc = "Estimate of the current bandwidth in bits per second";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the network adapter";

  public const string StatusInfoKey = "StatusInfo";
  public const string StatusInfoDesc = "State of the network adapter";

  public const string SystemCreationClassNameKey = "SystemCreationClassName";
  public const string SystemCreationClassNameDesc = "Value of the scoping computer system's CreationClassName property";

  public const string SystemNameKey = "SystemName";
  public const string SystemNameDesc = "Name of the scoping system";

  public const string TimeOfLastResetKey = "TimeOfLastReset";
  public const string TimeOfLastResetDesc = "Date and time the network adapter was last reset";

  public const string SpeedUnit = "bps";
}
