using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.UserAccount;

internal static class WmiUserAccount {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.UserAccount;

  // ---------------------------------------------------------------------
  // Shared Properties
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string InstallDate = CommonWmiProperties.InstallDate;
  public const string Name = CommonWmiProperties.Name;
  public const string Status = CommonWmiProperties.Status;

  // ---------------------------------------------------------------------
  // User Account Specific Properties
  // ---------------------------------------------------------------------
  public const string AccountType = nameof(AccountType);
  public const string Disabled = nameof(Disabled);
  public const string Domain = nameof(Domain);
  public const string FullName = nameof(FullName);
  public const string LocalAccount = nameof(LocalAccount);
  public const string Lockout = nameof(Lockout);
  public const string PasswordChangeable = nameof(PasswordChangeable);
  public const string PasswordExpires = nameof(PasswordExpires);
  public const string PasswordRequired = nameof(PasswordRequired);
  public const string SID = nameof(SID);
  public const string SIDType = nameof(SIDType);
}
