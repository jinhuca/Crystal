using Crystal.Infrastructure.DataStructures.Cpu.Interfaces;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cores;
using Crystal.Infrastructure.DataStructures.Cpu.Interfaces.Cpus;

namespace Crystal.Service.Cpu;

/// <summary>
/// Live CPU/core sensor readings sourced from the Telemetry provider. The
/// source owns the underlying hardware session, so it is disposable and is
/// refreshed between reads to re-sample the sensors.
/// </summary>
public interface ICpuTelemetrySource : IDisposable {
  /// <summary>
  /// Re-samples every enabled CPU sensor. Call before reading to get fresh values.
  /// </summary>
  void Refresh();

  /// <summary>
  /// Package-level sensors for the processor at <paramref name="socketIndex"/>,
  /// or <see langword="null"/> when telemetry has no matching processor.
  /// </summary>
  ICpuSensors? GetSensors(int socketIndex);

  /// <summary>
  /// Per-core sensor/topology rows for the processor at <paramref name="socketIndex"/>.
  /// Empty when telemetry has no matching processor.
  /// </summary>
  IReadOnlyList<ICoreInfo> GetCores(int socketIndex);
}
