using System;

namespace Crystal.Telemetry.Hardware.Motherboard.Lpc.EC;

public interface IEmbeddedControllerIO : IDisposable {
  void Read(ushort[] registers, byte[] data);
}