using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class EventLogConstants {
  public const string QueryString = "SELECT * FROM Win32_NTEventLogFile";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the event log file";

  public const string CreationClassNameKey = "CreationClassName";
  public const string CreationClassNameDesc = "Name of the first concrete class to appear in the inheritance chain";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the event log file";

  public const string FileSizeKey = "FileSize";
  public const string FileSizeDesc = "Size of the event log file, in bytes";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the event log file was installed";

  public const string LogfileNameKey = "LogfileName";
  public const string LogfileNameDesc = "Name of the event log, e.g. Application, System, Security";

  public const string MaxFileSizeKey = "MaxFileSize";
  public const string MaxFileSizeDesc = "Maximum size, in bytes, the event log file is allowed to reach";

  public const string NameKey = "Name";
  public const string NameDesc = "Inherited path name of the event log file";

  public const string NumberOfRecordsKey = "NumberOfRecords";
  public const string NumberOfRecordsDesc = "Number of records currently in the event log file";

  public const string OverwriteOutDatedKey = "OverwriteOutDated";
  public const string OverwriteOutDatedDesc = "Number of days a record is kept before it can be overwritten, if OverwritePolicy is set accordingly";

  public const string OverwritePolicyKey = "OverwritePolicy";
  public const string OverwritePolicyDesc = "Policy describing how the event log file handles records once it reaches its maximum size";

  public const string SourcesKey = "Sources";
  public const string SourcesDesc = "Array of the friendly, or programmatic, names of event sources that log events to this event log file";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the event log file object";
}
