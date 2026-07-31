using Crystal.Telemetry.PawnIo;
using System.Diagnostics;
using System.Text;

namespace Crystal.Telemetry.Hardware.Motherboard.Lpc.EC;

/// <summary>
/// Provides access to the ChromeOS Embedded Controller IO interface.
/// </summary>
public class ChromeOSEmbeddedControllerIO : IEmbeddedControllerIO {
  private const short EC_CMD_TEMP_SENSOR_GET_INFO = 0x0070;

  private readonly LpcCrOSEc _pawnModule;
  private bool _disposed;

  /// <summary>
  /// Initializes a new instance of the <see cref="ChromeOSEmbeddedControllerIO"/> class and acquires the EC bus lock.
  /// </summary>
  /// <exception cref="BusMutexLockingFailedException">Thrown when the EC bus mutex could not be acquired.</exception>
  public ChromeOSEmbeddedControllerIO() {
    _pawnModule = new LpcCrOSEc();

    if (!Mutexes.WaitEc(10)) {
      throw new BusMutexLockingFailedException();
    }
  }

  /// <summary>
  /// Reads one byte for each of the specified registers into the provided data buffer.
  /// </summary>
  /// <param name="registers">The register offsets to read.</param>
  /// <param name="data">The buffer that receives the read bytes; must be at least as long as <paramref name="registers"/>.</param>
  public void Read(ushort[] registers, byte[] data) {
    Trace.Assert(registers.Length <= data.Length,
                 "data buffer length has to be greater or equal to the registers array length");

    for (int i = 0; i < registers.Length; i++) {
      data[i] = ReadMemmap((byte)registers[i], 1)[0];
    }
  }

  /// <summary>
  /// Reads a range of bytes from the EC memory-mapped region.
  /// </summary>
  /// <param name="offset">The starting offset within the memory-mapped region.</param>
  /// <param name="bytes">The number of bytes to read.</param>
  /// <returns>The bytes read from the memory-mapped region.</returns>
  public byte[] ReadMemmap(byte offset, byte bytes) {
    return _pawnModule.ReadMemmap(offset, bytes);
  }

  /// <summary>
  /// Retrieves the name of the temperature sensor at the specified index.
  /// </summary>
  /// <param name="index">The zero-based temperature sensor index.</param>
  /// <returns>The sensor name, or a fallback name if the query fails.</returns>
  public string TempSensorGetName(byte index) {
    try {
      byte[] resp = _pawnModule.EcCommand(0, EC_CMD_TEMP_SENSOR_GET_INFO, 1, 33, [index]);
      //byte sensorType = resp[33];
      return Encoding.ASCII.GetString(resp, 0, 32).TrimEnd('\0');
    }
    catch (System.Exception) {
      return "Temp " + index;
    }
  }

  /// <summary>
  /// Releases the EC bus lock held by this instance.
  /// </summary>
  public void Dispose() {
    if (!_disposed) {
      _disposed = true;
      Mutexes.ReleaseEc();
    }
  }

  /// <summary>
  /// The exception thrown when the ISA bus mutex could not be locked.
  /// </summary>
  public class BusMutexLockingFailedException : EmbeddedController.IOException {
    /// <summary>
    /// Initializes a new instance of the <see cref="BusMutexLockingFailedException"/> class.
    /// </summary>
    public BusMutexLockingFailedException()
        : base("could not lock ISA bus mutex") { }
  }
}
