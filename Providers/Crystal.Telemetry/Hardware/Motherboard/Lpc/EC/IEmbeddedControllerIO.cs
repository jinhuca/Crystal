using System;

namespace Crystal.Telemetry.Hardware.Motherboard.Lpc.EC;

/// <summary>
/// Provides low-level read access to embedded controller registers.
/// </summary>
public interface IEmbeddedControllerIO : IDisposable {
  /// <summary>
  /// Reads the values of the specified embedded controller registers.
  /// </summary>
  /// <param name="registers">The register addresses to read.</param>
  /// <param name="data">The buffer that receives the value read from each corresponding register.</param>
  void Read(ushort[] registers, byte[] data);
}