namespace ResourceModule.Controls.Meter;

public enum Unit {
  None,
  Percent,
  Absolute,
  GHz,
  Celsius,
  Volts,
  Watts
}

// Display strings used by UnitToDisplayConverter. Adjust defaults as needed.
public static class Definitions {
  public static string? NoneString { get; set; } = string.Empty;
  public static string? AbsoluteString { get; set; } = "units";
  public static string? PercentageString { get; set; } = "%";
  public static string? GHzString { get; set; } = "GHz";
  public static string? CelsiusString { get; set; } = "°C";
  public static string? VoltsString { get; set; } = "V";
  public static string? WattsString { get; set; } = "W";
}