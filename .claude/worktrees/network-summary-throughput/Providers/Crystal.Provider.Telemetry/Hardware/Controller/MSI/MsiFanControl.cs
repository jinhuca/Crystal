namespace Crystal.Provider.Telemetry.Hardware.Controller.MSI;

/// <summary>
/// Represents the state of the MSI fan controller, exposing the individual fans and
/// the temperature readings reported by the controller.
/// </summary>
public class MsiFanControl {
  /// <summary>Gets or sets the first fan.</summary>
  public MsiFan Fan1 { get; set; } = new();

  /// <summary>Gets or sets the second fan.</summary>
  public MsiFan Fan2 { get; set; } = new();

  /// <summary>Gets or sets the third fan.</summary>
  public MsiFan Fan3 { get; set; } = new();

  /// <summary>Gets or sets the fourth fan.</summary>
  public MsiFan Fan4 { get; set; } = new();

  /// <summary>Gets or sets the fifth fan.</summary>
  public MsiFan Fan5 { get; set; } = new();

  /// <summary>Gets or sets the inlet temperature in degrees Celsius.</summary>
  public int TemperatureInlet { get; set; }

  /// <summary>Gets or sets the outlet temperature in degrees Celsius.</summary>
  public int TemperatureOutlet { get; set; }

  /// <summary>Gets or sets the reading of the first temperature sensor in degrees Celsius.</summary>
  public int TemperatureSensor1 { get; set; }

  /// <summary>Gets or sets the reading of the second temperature sensor in degrees Celsius.</summary>
  public int TemperatureSensor2 { get; set; }
}
