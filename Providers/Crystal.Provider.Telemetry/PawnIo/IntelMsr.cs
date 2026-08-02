using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Provider.Telemetry.PawnIo;

/// <summary>
/// Provides access to Intel model-specific registers (MSRs) via the PawnIO driver.
/// </summary>
public class IntelMsr {
  private readonly long[] _inArray = new long[1];
  private readonly PawnIo _pawnIO = PawnIo.LoadModuleFromResource(typeof(IntelMsr).Assembly, $"{nameof(Crystal.Provider.Telemetry)}.Resources.PawnIo.IntelMSR.bin");

  /// <summary>
  /// Reads the full 64-bit value of the specified model-specific register.
  /// </summary>
  /// <param name="index">The MSR index to read.</param>
  /// <param name="value">When this method returns, contains the register value, or zero on failure.</param>
  /// <returns><see langword="true"/> if the read succeeded; otherwise, <see langword="false"/>.</returns>
  public bool ReadMsr(uint index, out ulong value) {
    _inArray[0] = index;
    value = 0;
    try {
      long[] outArray = _pawnIO.Execute("ioctl_read_msr", _inArray, 1);
      value = (ulong)outArray[0];
    }
    catch {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Reads the specified model-specific register into its low (EAX) and high (EDX) 32-bit halves.
  /// </summary>
  /// <param name="index">The MSR index to read.</param>
  /// <param name="eax">When this method returns, contains the low 32 bits, or zero on failure.</param>
  /// <param name="edx">When this method returns, contains the high 32 bits, or zero on failure.</param>
  /// <returns><see langword="true"/> if the read succeeded; otherwise, <see langword="false"/>.</returns>
  public bool ReadMsr(uint index, out uint eax, out uint edx) {
    _inArray[0] = index;
    eax = 0;
    edx = 0;
    try {
      long[] outArray = _pawnIO.Execute("ioctl_read_msr", _inArray, 1);
      eax = (uint)outArray[0];
      edx = (uint)(outArray[0] >> 32);
    }
    catch {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Reads the specified model-specific register on the processor identified by the given affinity.
  /// </summary>
  /// <param name="index">The MSR index to read.</param>
  /// <param name="eax">When this method returns, contains the low 32 bits, or zero on failure.</param>
  /// <param name="edx">When this method returns, contains the high 32 bits, or zero on failure.</param>
  /// <param name="affinity">The thread affinity that selects the target processor.</param>
  /// <returns><see langword="true"/> if the read succeeded; otherwise, <see langword="false"/>.</returns>
  public bool ReadMsr(uint index, out uint eax, out uint edx, GroupAffinity affinity) {
    GroupAffinity previousAffinity = ThreadAffinity.Set(affinity);
    bool result = ReadMsr(index, out eax, out edx);
    ThreadAffinity.Set(previousAffinity);
    return result;
  }

  /// <summary>
  /// Closes the underlying PawnIO module.
  /// </summary>
  public void Close() => _pawnIO.Close();
}
