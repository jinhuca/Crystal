using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.NetworkLoginProfile;

internal static class WmiNetworkLoginProfile {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.NetworkLoginProfile;

  // ---------------------------------------------------------------------
  // Shared Properties (CIM_Setting)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;

  // ---------------------------------------------------------------------
  // Network Login Profile Specific Properties
  // ---------------------------------------------------------------------
  public const string AccountExpires = nameof(AccountExpires);
  public const string AuthorizationFlags = nameof(AuthorizationFlags);
  public const string BadPasswordCount = nameof(BadPasswordCount);
  public const string CodePage = nameof(CodePage);
  public const string Comment = nameof(Comment);
  public const string CountryCode = nameof(CountryCode);
  public const string Flags = nameof(Flags);
  public const string FullName = nameof(FullName);
  public const string HomeDirectory = nameof(HomeDirectory);
  public const string HomeDirectoryDrive = nameof(HomeDirectoryDrive);
  public const string LastLogoff = nameof(LastLogoff);
  public const string LastLogon = nameof(LastLogon);
  public const string LogonHours = nameof(LogonHours);
  public const string LogonServer = nameof(LogonServer);
  public const string MaximumStorage = nameof(MaximumStorage);
  public const string NumberOfLogons = nameof(NumberOfLogons);
  public const string Parameters = nameof(Parameters);
  public const string PasswordAge = nameof(PasswordAge);
  public const string PasswordExpires = nameof(PasswordExpires);
  public const string PrimaryGroupId = nameof(PrimaryGroupId);
  public const string Privileges = nameof(Privileges);
  public const string Profile = nameof(Profile);
  public const string ScriptPath = nameof(ScriptPath);
  public const string SettingID = nameof(SettingID);
  public const string UnitsPerWeek = nameof(UnitsPerWeek);
  public const string UserComment = nameof(UserComment);
  public const string UserId = nameof(UserId);
  public const string UserType = nameof(UserType);
  public const string Workstations = nameof(Workstations);
}
