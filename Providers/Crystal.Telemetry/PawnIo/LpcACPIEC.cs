namespace Crystal.Telemetry.PawnIo;

/// <summary>
/// Provides access to the ACPI embedded controller via the PawnIO LpcACPIEC module.
/// </summary>
public class LpcAcpiEc {
  private readonly PawnIo _pawnIO = PawnIo.LoadModuleFromResource(typeof(AmdFamily0F).Assembly, $"{nameof(Crystal.Telemetry)}.Resources.PawnIo.LpcACPIEC.bin");

  /// <summary>
  /// Reads a byte from the specified embedded controller port.
  /// </summary>
  /// <param name="port">The port to read from.</param>
  /// <returns>The value read from the port.</returns>
  public byte ReadPort(byte port) {
    long[] inArray = new long[1];
    inArray[0] = port;
    long[] outArray = _pawnIO.Execute("ioctl_pio_read", inArray, 1);
    return (byte)outArray[0];
  }

  /// <summary>
  /// Writes a byte to the specified embedded controller port.
  /// </summary>
  /// <param name="port">The port to write to.</param>
  /// <param name="value">The value to write.</param>
  public void WritePort(byte port, byte value) {
    long[] inArray = new long[2];
    inArray[0] = port;
    inArray[1] = value;
    _pawnIO.Execute("ioctl_pio_write", inArray, 0);
  }

  /// <summary>
  /// Closes the underlying PawnIO module.
  /// </summary>
  public void Close() => _pawnIO.Close();
}
