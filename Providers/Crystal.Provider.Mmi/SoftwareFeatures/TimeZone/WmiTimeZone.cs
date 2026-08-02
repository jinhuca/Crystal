using Crystal.Provider.Mmi.Wmi;

namespace Crystal.Provider.Mmi.SoftwareFeatures.TimeZone;

internal static class WmiTimeZone {
  // ---------------------------------------------------------------------
  // WMI Class
  // ---------------------------------------------------------------------
  public const string ClassName = WmiClasses.TimeZone;

  // ---------------------------------------------------------------------
  // Shared Properties (CIM_Setting)
  // ---------------------------------------------------------------------
  public const string Caption = CommonWmiProperties.Caption;
  public const string Description = CommonWmiProperties.Description;

  // ---------------------------------------------------------------------
  // Time Zone Specific Properties
  // ---------------------------------------------------------------------
  public const string Bias = nameof(Bias);
  public const string DaylightBias = nameof(DaylightBias);
  public const string DaylightDay = nameof(DaylightDay);
  public const string DaylightDayOfWeek = nameof(DaylightDayOfWeek);
  public const string DaylightHour = nameof(DaylightHour);
  public const string DaylightMillisecond = nameof(DaylightMillisecond);
  public const string DaylightMinute = nameof(DaylightMinute);
  public const string DaylightMonth = nameof(DaylightMonth);
  public const string DaylightName = nameof(DaylightName);
  public const string DaylightSecond = nameof(DaylightSecond);
  public const string DaylightYear = nameof(DaylightYear);
  public const string SettingID = nameof(SettingID);
  public const string StandardBias = nameof(StandardBias);
  public const string StandardDay = nameof(StandardDay);
  public const string StandardDayOfWeek = nameof(StandardDayOfWeek);
  public const string StandardHour = nameof(StandardHour);
  public const string StandardMillisecond = nameof(StandardMillisecond);
  public const string StandardMinute = nameof(StandardMinute);
  public const string StandardMonth = nameof(StandardMonth);
  public const string StandardName = nameof(StandardName);
  public const string StandardSecond = nameof(StandardSecond);
  public const string StandardYear = nameof(StandardYear);
}
