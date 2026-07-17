using System;

namespace Crystal.Telemetry.Hardware.Motherboard.Lpc;

internal interface IGigabyteController : IDisposable {
  bool Enable(bool enabled);

  void Restore();
}
