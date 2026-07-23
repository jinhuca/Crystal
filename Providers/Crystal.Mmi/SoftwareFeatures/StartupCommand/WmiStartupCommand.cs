using Crystal.Mmi.Wmi;

namespace Crystal.Mmi.SoftwareFeatures.StartupCommand;

internal static class WmiStartupCommand {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.StartupCommand;

  // ---------------------------------------------------------------------
  // Shared Properties (CIM_Setting)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;
  public const string Name = CommonWmiProperties.Name;

  // ---------------------------------------------------------------------
  // Startup Command Specific Properties
  // ---------------------------------------------------------------------
  public const string Command = nameof(Command);
  public const string Location = nameof(Location);
  public const string SettingID = nameof(SettingID);
  public const string User = nameof(User);
  public const string UserSID = nameof(UserSID);
}
