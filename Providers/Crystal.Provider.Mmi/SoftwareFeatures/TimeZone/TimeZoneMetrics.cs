namespace Crystal.Provider.Mmi.SoftwareFeatures.TimeZone;

public record TimeZoneMetrics(
  int? Bias,                    // minutes; UTC = local time - Bias
  string? Caption,
  int? DaylightBias,             // minutes, added to Bias during daylight saving time
  uint? DaylightDay,
  byte? DaylightDayOfWeek,
  uint? DaylightHour,
  uint? DaylightMillisecond,
  uint? DaylightMinute,
  uint? DaylightMonth,
  string? DaylightName,
  uint? DaylightSecond,
  uint? DaylightYear,
  string? Description,
  string? SettingID,
  uint? StandardBias,
  uint? StandardDay,
  byte? StandardDayOfWeek,
  uint? StandardHour,
  uint? StandardMillisecond,
  uint? StandardMinute,
  uint? StandardMonth,
  string? StandardName,
  uint? StandardSecond,
  uint? StandardYear
);
