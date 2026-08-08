namespace Crystal.Provider.Telemetry.PawnIo;

/// <summary>
/// Provides access to AMD Family 17h (and later) processor registers via the PawnIO driver.
/// </summary>
public class AmdFamily17 {
  private readonly PawnIo _pawnIo = PawnIo.LoadModuleFromResource(typeof(AmdFamily0F).Assembly, "Crystal.Provider.Telemetry.Resources.PawnIo.AMDFamily17.bin");

  /// <summary>
  /// Reads a value from the System Management Network (SMN) at the specified offset.
  /// </summary>
  /// <param name="offset">The SMN address offset to read.</param>
  /// <returns>The value read from the SMN.</returns>
  public uint ReadSmn(uint offset) {
    long[] result = _pawnIo.Execute("ioctl_read_smn", [offset], 1);
    return (uint)result[0];
  }

  /// <summary>
  /// Reads the specified model-specific register into its low (EAX) and high (EDX) 32-bit halves.
  /// </summary>
  /// <param name="index">The MSR index to read.</param>
  /// <param name="eax">When this method returns, contains the low 32 bits, or zero on failure.</param>
  /// <param name="edx">When this method returns, contains the high 32 bits, or zero on failure.</param>
  /// <returns><see langword="true"/> if the read succeeded; otherwise, <see langword="false"/>.</returns>
  public bool ReadMsr(uint index, out uint eax, out uint edx) {
    long[] inArray = new long[1];
    inArray[0] = index;
    eax = 0;
    edx = 0;
    try {
      long[] outArray = _pawnIo.Execute("ioctl_read_msr", inArray, 1);
      eax = (uint)outArray[0];
      edx = (uint)(outArray[0] >> 32);
    }
    catch {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Reads the full 64-bit value of the specified model-specific register.
  /// </summary>
  /// <param name="index">The MSR index to read.</param>
  /// <param name="eaxedx">When this method returns, contains the register value, or zero on failure.</param>
  /// <returns><see langword="true"/> if the read succeeded; otherwise, <see langword="false"/>.</returns>
  public bool ReadMsr(uint index, out ulong eaxedx) {
    long[] inArray = new long[1];
    inArray[0] = index;
    eaxedx = 0;
    try {
      long[] outArray = _pawnIo.Execute("ioctl_read_msr", inArray, 1);
      eaxedx = (ulong)outArray[0];
    }
    catch {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Closes the underlying PawnIO module.
  /// </summary>
  public void Close() => _pawnIo.Close();
}
