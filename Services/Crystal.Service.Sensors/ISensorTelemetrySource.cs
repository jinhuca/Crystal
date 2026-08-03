using Crystal.Infrastructure.DataStructures.Sensors;

namespace Crystal.Service.Sensors;

/// <summary>
/// Live system sensor readings sourced from the Telemetry provider. The source
/// owns the underlying hardware session, so it is disposable and is re-sampled
/// on every read.
/// </summary>
public interface ISensorTelemetrySource : IDisposable {
  /// <summary>
  /// Re-samples every enabled hardware sensor and returns a flat snapshot of the
  /// current readings across all hardware. Call once per poll.
  /// </summary>
  IReadOnlyList<SensorReading> Read();
}
