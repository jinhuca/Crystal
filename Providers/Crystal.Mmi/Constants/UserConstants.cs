using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Mmi.Constants; 
internal static class UserConstants {
  public const string QueryString = "SELECT * FROM Win32_UserAccount";

  public const string AccountTypeKey = "AccountType";
  public const string AccountTypeDesc = "Bitmask of flags describing the account type (e.g. temporary, normal, interdomain trust)";

  public const string CaptionKey = "Caption";
  public const string CaptionDesc = "Short description of the user account";

  public const string DescriptionKey = "Description";
  public const string DescriptionDesc = "Description of the user account";

  public const string DisabledKey = "Disabled";
  public const string DisabledDesc = "If True, the user account is disabled";

  public const string DomainKey = "Domain";
  public const string DomainDesc = "Name of the Windows domain, or computer name for a local account, to which the user account belongs";

  public const string FullNameKey = "FullName";
  public const string FullNameDesc = "Full name of the local user";

  public const string InstallDateKey = "InstallDate";
  public const string InstallDateDesc = "Date and time the user account was installed";

  public const string LocalAccountKey = "LocalAccount";
  public const string LocalAccountDesc = "If True, the account is local to the computer rather than a domain account";

  public const string LockoutKey = "Lockout";
  public const string LockoutDesc = "If True, the user account is currently locked out";

  public const string NameKey = "Name";
  public const string NameDesc = "User's logon name";

  public const string PasswordChangeableKey = "PasswordChangeable";
  public const string PasswordChangeableDesc = "If True, the user's password can be changed";

  public const string PasswordExpiresKey = "PasswordExpires";
  public const string PasswordExpiresDesc = "If True, the user's password expires";

  public const string PasswordRequiredKey = "PasswordRequired";
  public const string PasswordRequiredDesc = "If True, a password is required for the user account";

  public const string SIDKey = "SID";
  public const string SIDDesc = "Security identifier (SID) of the user account";

  public const string SIDTypeKey = "SIDType";
  public const string SIDTypeDesc = "Enumerated type describing what the SID references (user, group, alias, etc.)";

  public const string StatusKey = "Status";
  public const string StatusDesc = "Current status of the user account object";
}
