using DiskInfoToolkit;

namespace Crystal.Telemetry.Hardware.Storage;

/// <summary>
/// Represents a single S.M.A.R.T. attribute of a storage device and the optional
/// sensor that exposes its value.
/// </summary>
public sealed class SmartAttribute {
  /// <summary>
  /// Initializes a new instance of the <see cref="SmartAttribute"/> class.
  /// </summary>
  /// <param name="smartAttribute">The SMART attribute.</param>
  /// <param name="sensorType">
  /// Type of the sensor or null if no sensor is to
  /// be created.
  /// </param>
  /// <param name="sensorChannel">
  /// If there exists more than one attribute with
  /// the same sensor channel and type, then a sensor is created only for the
  /// first attribute.
  /// </param>
  /// <param name="sensorName">
  /// The name to be used for the sensor, or null if
  /// no sensor is created.
  /// </param>
  /// <param name="defaultHiddenSensor">True to hide the sensor initially.</param>
  public SmartAttribute(SmartAttributeEntry smartAttribute, SensorType? sensorType, int sensorChannel, string sensorName, bool defaultHiddenSensor = false) {
    Attribute = smartAttribute;
    SensorType = sensorType;
    SensorChannel = sensorChannel;
    SensorName = sensorName ?? Name;
    IsHiddenByDefault = defaultHiddenSensor;
  }

  /// <summary>Gets the underlying raw S.M.A.R.T. attribute entry.</summary>
  public SmartAttributeEntry Attribute { get; internal set; }

  /// <summary>Gets the identifier of the S.M.A.R.T. attribute.</summary>
  public byte Id => Attribute.ID;

  /// <summary>Gets the name of the S.M.A.R.T. attribute.</summary>
  public string Name => Attribute.Name;

  /// <summary>Gets the type of the sensor associated with this attribute, or null if none.</summary>
  public SensorType? SensorType { get; }

  /// <summary>Gets the sensor channel used when creating the sensor for this attribute.</summary>
  public int SensorChannel { get; }

  /// <summary>Gets the name of the sensor associated with this attribute.</summary>
  public string SensorName { get; }

  /// <summary>Gets a value indicating whether the sensor is hidden by default.</summary>
  public bool IsHiddenByDefault { get; }

  /// <summary>Gets the current raw value of the S.M.A.R.T. attribute.</summary>
  public float Value => Attribute.RawValue;

  /// <summary>Gets the threshold value of the S.M.A.R.T. attribute.</summary>
  public byte Threshold => Attribute.ThresholdValue;
}
