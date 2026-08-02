using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Provider.Telemetry.PawnIo;

/// <summary>
/// Provides access to AMD Family 10h processor registers and residency information
/// through the PawnIO kernel driver module.
/// </summary>
public class AmdFamily10 {
  private readonly PawnIo _pawnIo = PawnIo.LoadModuleFromResource(typeof(AmdFamily0F).Assembly, $"{nameof(Crystal.Provider.Telemetry)}.Resources.PawnIo.AMDFamily10.bin");

  /// <summary>
  /// Measures the time-stamp counter multiplier.
  /// </summary>
  /// <param name="ctrPerTick">Receives the counter value per tick.</param>
  /// <param name="cofVid">Receives the current operating frequency / voltage identifier value.</param>
  public void MeasureTscMultiplier(out long ctrPerTick, out long cofVid) {
    long[] result = _pawnIo.Execute("ioctl_measure_tsc_multiplier", [], 2);
    ctrPerTick = result[0];
    cofVid = result[1];
  }

  /// <summary>
  /// Determines whether C-state residency information can be read from the processor.
  /// </summary>
  /// <returns><c>true</c> if C-state residency information is available; otherwise, <c>false</c>.</returns>
  public bool HaveCstateResidencyInfo() {
    try {
      ReadCstateResidency();
      return true;
    }
    catch {
      // ignored
    }

    return false;
  }

  /// <summary>
  /// Reads the C-state residency values from the processor.
  /// </summary>
  /// <returns>An array containing the C-state residency bytes.</returns>
  public byte[] ReadCstateResidency() {
    long[] result = _pawnIo.Execute("ioctl_read_cstate_residency", [], 2);
    return [(byte)result[0], (byte)result[1]];
  }

  /// <summary>
  /// Reads a value from the miscellaneous control register.
  /// </summary>
  /// <param name="cpu">The index of the processor to read from.</param>
  /// <param name="offset">The register offset to read.</param>
  /// <returns>The value read from the miscellaneous control register.</returns>
  public uint ReadMiscCtl(int cpu, uint offset) {
    long[] result = _pawnIo.Execute("ioctl_read_miscctl", [cpu, offset], 1);
    return (uint)result[0];
  }

  /// <summary>
  /// Reads a value from the System Management Unit (SMU).
  /// </summary>
  /// <param name="offset">The SMU register offset to read.</param>
  /// <returns>The value read from the SMU.</returns>
  public uint ReadSmu(uint offset) {
    long[] result = _pawnIo.Execute("ioctl_read_smu", [offset], 1);
    return (uint)result[0];
  }

  /// <summary>
  /// Reads a model-specific register (MSR).
  /// </summary>
  /// <param name="index">The index of the MSR to read.</param>
  /// <param name="eax">Receives the low 32 bits of the register value.</param>
  /// <param name="edx">Receives the high 32 bits of the register value.</param>
  /// <returns><c>true</c> if the register was read successfully; otherwise, <c>false</c>.</returns>
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
  /// Reads a model-specific register (MSR) on the thread identified by the specified affinity.
  /// </summary>
  /// <param name="index">The index of the MSR to read.</param>
  /// <param name="eax">Receives the low 32 bits of the register value.</param>
  /// <param name="edx">Receives the high 32 bits of the register value.</param>
  /// <param name="affinity">The group affinity of the processor to read the register on.</param>
  /// <returns><c>true</c> if the register was read successfully; otherwise, <c>false</c>.</returns>
  public bool ReadMsr(uint index, out uint eax, out uint edx, GroupAffinity affinity) {
    GroupAffinity previousAffinity = ThreadAffinity.Set(affinity);
    bool result = ReadMsr(index, out eax, out edx);
    ThreadAffinity.Set(previousAffinity);
    return result;
  }

  /// <summary>
  /// Closes the underlying PawnIO module and releases its resources.
  /// </summary>
  public void Close() => _pawnIo.Close();
}
