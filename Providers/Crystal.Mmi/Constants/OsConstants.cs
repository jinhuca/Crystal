using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal class OsConstants {
  public const string QueryString = "SELECT * FROM Win32_OperatingSystem";

  public const string BootDeviceKey = "BootDevice";
  public const string BootDeviceDesc = "Name of the disk drive from which the Windows operating system starts";

  public const string BuildNumberKey = "BuildNumber";
  public const string BuildNumberDesc = "Build number of an operating system";

  public const string BuildTypeKey = "BuildType";
  public const string BuildTypeDesc = "Type of build used for an operating system";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of OS";

  public const string CodeSetKey = "CodeSet";
  public const string CodeSetDesc = "Code page value an operating system uses";

  public const string CountryCodeKey = "CountryCode";
  public const string CountryCodeDesc = "Code for the country/region that an operating system uses";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Name of the first concrete class that appears in the inheritance chain used in the creation of an instance";

  public const string CSCreationClassNameKey = "CSCreationClassName";
  public const string CSCreationClassNameDesc = "Creation class name of the scoping computer system";

  public const string CSDVersionKey = "CSDVersion";
  public const string CSDVersionDesc = "Latest service pack installed";

  public const string CSNameKey = "CSName";
  public const string CSNameDesc = "Name of the scoping computer system";

  public const string VersionKey = "Version";
  public const string VersionDesc = "Version number of the operating system";

  public const string CurrentTimeZoneKey = "CurrentTimeZone";
  public const string CurrentTimeZoneDesc = "Number, in minutes, an operating system is offset from Greenwich mean time (GMT)";

  public const string DataExecutionPrevention_32BitApplicationsKey = "DataExecutionPrevention_32BitApplications";
  public const string DataExecutionPrevention_32BitApplicationsDesc = "Availability of data execution prevention hardware feature";

  public const string DataExecutionPrevention_AvailableKey = "DataExecutionPrevention_Available";
  public const string DataExecutionPrevention_AvailableDesc = "Availability of of data execution prevention";

  public const string DataExecutionPrevention_DriversKey = "DataExecutionPrevention_Drivers";
  public const string DataExecutionPrevention_DriversDesc = "When the data execution prevention hardware feature is available, this property indicates that the feature is set to work for drivers if True";

  public const string LastBootUpTimeKey = "LastBootUpTime";
  public const string LastBootUpTimeDesc = "Date and time the operating system was last restarted";

  public const string LocalDateTimeKey = "LocalDateTime";
  public const string LocalDateTimeDesc = "Operating system version of the local date and time-of-day";

  public const string LocaleKey = "Locale";
  public const string LocaleDesc = "Language identifier used by the operating system";

  public const string ManufacturerKey = "Manufacturer";
  public const string ManufacturerDesc = "Name of the operating system manufacturer";

  public const string MaxNumberOfProcessesKey = "MaxNumberOfProcesses";
  public const string MaxNumberOfProcessesDesc = "Maximum number of process contexts the operating system can support";

  public const string MaxProcessMemorySizeKey = "MaxProcessMemorySize";
  public const string MaxProcessMemorySizeDesc = "Maximum number, in kilobytes, of memory that can be allocated to a process";

  public const string MUILanguagesKey = "MUILanguages";
  public const string MUILanguagesDesc = "Multilingual User Interface Pack (MUI Pack ) languages installed on the computer";

  public const string NameKey = "Name";
  public const string NameDesc = "Operating system instance within a computer system";

  public const string NumberOfLicensedUsersKey = "NumberOfLicensedUsers";
  public const string NumberOfLicensedUsersDesc = "Number of user licenses for the operating system";

  public const string NumberOfProcessesKey = "NumberOfProcesses";

  public const string NumberOfUsersKey = "NumberOfUsers";
  public const string NumberOfUsersDesc = "Number of user sessions for which the operating system is storing state information currently";

  public const string OperatingSystemSKUKey = "OperatingSystemSKU";
  public const string OperatingSystemSKUDesc = "Stock Keeping Unit (SKU) number for the operating system";

  public const string OrganizationKey = "Organization";
  public const string OrganizationDesc = "Company name for the registered user of the operating system";

  public const string OSArchitectureKey = "OSArchitecture";
  public const string OSArchitectureDesc = "Architecture of the operating system";

  public const string OSLanguageKey = "OSLanguage";
  public const string OSLanguageDesc = "Language version of the operating system installed";

  public const string OSProductSuiteKey = "OSProductSuite";
  public const string OSProductSuiteDesc = "Installed and licensed system product additions to the operating system";

  public const string OSTypeKey = "OSType";
  public const string OSTypeDesc = "Type of operating system";

  public const string OtherTypeDescriptionKey = "OtherTypeDescription";
  public const string OtherTypeDescriptionDesc = "Additional description for the current operating system version";

  public const string PAEEnabledKey = "PAEEnabled";
  public const string PAEEnabledDesc = "Physical Address Extensions";

  public const string PortableOperatingSystemKey = "PortableOperatingSystem";
  public const string PortableOperatingSystemDesc = "Specifies whether the operating system booted from an external USB device";

  public const string PrimaryKey = "Primary";
  public const string PrimaryDesc = "Specifies whether this is the primary operating system";

  public const string ProductTypeKey = "ProductType";
  public const string ProductTypeDesc = "Additional system information";

  public const string RegisteredUserKey = "RegisteredUser";
  public const string RegisteredUserDesc = "Name of the registered user of the operating system";

  public const string SerialNumberKey = "SerialNumber";
  public const string SerialNumberDesc = "Operating system product serial identification number";
}
