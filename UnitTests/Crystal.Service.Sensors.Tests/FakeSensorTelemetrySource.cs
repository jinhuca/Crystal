using Crystal.Infrastructure.DataStructures.Sensors;

namespace Crystal.Service.Sensors.Tests;

/// <summary>
/// In-memory <see cref="ISensorTelemetrySource"/> that records how often it was
/// polled and disposed, so tests need no real hardware session.
/// </summary>
internal sealed class FakeSensorTelemetrySource : ISensorTelemetrySource {
  private readonly Func<int, IReadOnlyList<SensorReading>> _factory;

  public FakeSensorTelemetrySource(IReadOnlyList<SensorReading> readings)
      : this(_ => readings) { }

  public FakeSensorTelemetrySource(Func<int, IReadOnlyList<SensorReading>> factory)
      => _factory = factory;

  public int ReadCount { get; private set; }
  public bool Disposed { get; private set; }

  public IReadOnlyList<SensorReading> Read() => _factory(ReadCount++);

  public void Dispose() => Disposed = true;

  public static SensorReading Reading(HardwareType hardwareType, string sensorName,
                                      SensorType sensorType = SensorType.Temperature, float? value = 42f) =>
      new(hardwareType.ToString(), hardwareType, sensorName, sensorType, value, null, null,
          SensorReadingExtensions.UnitFor(sensorType));
}
