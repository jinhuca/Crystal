using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class GroupConstants {
  public const string QueryString = "SELECT * FROM Win32_Group";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the group account";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the group account";

  public const string DomainKey = "Domain";
  public const string DomainDesc = "Name of the Windows domain, or computer name for a local group, to which the group account belongs";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the group account was installed";

  public const string LocalAccountKey = "LocalAccount";
  public const string LocalAccountDesc = "If True, the group account is local to the computer rather than a domain group";

  public const string NameKey = "Name";
  public const string NameDesc = "Name of the group account";

  public const string SIDKey = "SID";
  public const string SIDDesc = "Security identifier (SID) of the group account";

  public const string SIDTypeKey = "SIDType";
  public const string SIDTypeDesc = "Enumerated type describing what the SID references (user, group, alias, etc.)";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the group account object";
}
