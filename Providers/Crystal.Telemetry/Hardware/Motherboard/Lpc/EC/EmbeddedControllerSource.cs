namespace Crystal.Telemetry.Hardware.Motherboard.Lpc.EC;

/// <summary>
/// Describes a single embedded controller sensor source, including its register
/// location and the scaling used to convert the raw reading into a sensor value.
/// </summary>
/// <param name="name">The display name of the sensor.</param>
/// <param name="type">The type of sensor represented by this source.</param>
/// <param name="register">The base embedded controller register address to read from.</param>
/// <param name="size">The number of bytes to read from the register.</param>
/// <param name="factor">The multiplier applied to the raw register value.</param>
/// <param name="offset">The offset added to the scaled register value.</param>
/// <param name="blank">The raw register value that indicates a missing or invalid reading.</param>
/// <param name="isLittleEndian">Whether multi-byte readings are stored in little-endian order.</param>
public class EmbeddedControllerSource(string name, SensorType type, ushort register, byte size = 1, float factor = 1.0f, float offset = 0.0f, int blank = int.MaxValue, bool isLittleEndian = false) {
  /// <summary>Gets the raw register value that indicates a missing or invalid reading.</summary>
  public int Blank { get; } = blank;

  /// <summary>Gets the multiplier applied to the raw register value to produce the sensor value.</summary>
  public float Factor { get; } = factor;

  /// <summary>Gets a value indicating whether multi-byte readings are stored in little-endian order.</summary>
  public bool IsLittleEndian { get; } = isLittleEndian;

  /// <summary>Gets the display name of the sensor.</summary>
  public string Name { get; } = name;

  /// <summary>Gets the offset added to the scaled register value to produce the sensor value.</summary>
  public float Offset { get; } = offset;

  /// <summary>Gets the base embedded controller register address to read from.</summary>
  public ushort Register { get; } = register;

  /// <summary>Gets the number of bytes to read from the register.</summary>
  public byte Size { get; } = size;

  /// <summary>Gets the type of sensor represented by this source.</summary>
  public SensorType Type { get; } = type;
}
