using Crystal.Provider.Telemetry.Hardware;

namespace Crystal.Provider.Telemetry.PawnIo;

/// <summary>
/// Provides low-level access to AMD Family 0F (K10) CPU features via the PawnIO driver.
/// </summary>
public class AmdFamily0F {
  private readonly PawnIo _pawnIO = PawnIo.LoadModuleFromResource(typeof(AmdFamily0F).Assembly, "Crystal.Provider.Telemetry.Resources.PawnIo.AMDFamily0F.bin");
  /// <summary>
  /// Reads the value of a Model-Specific Register (MSR) for AMD Family 0F CPUs.
  /// </summary>
  /// <param name="index"></param>
  /// <param name="eax"></param>
  /// <param name="edx"></param>
  /// <returns></returns>
  public bool ReadMsr(uint index, out uint eax, out uint edx) {
    long[] inArray = new long[1];
    inArray[0] = index;
    eax = 0;
    edx = 0;
    try {
      long[] outArray = _pawnIO.Execute("ioctl_read_msr", inArray, 1);
      eax = (uint)outArray[0];
      edx = (uint)(outArray[0] >> 32);
    }
    catch {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Reads the value of a Model-Specific Register (MSR) for AMD Family 0F CPUs on a specific processor group and core.
  /// </summary>
  /// <param name="index"></param>
  /// <param name="eax"></param>
  /// <param name="edx"></param>
  /// <param name="affinity"></param>
  /// <returns></returns>
  public bool ReadMsr(uint index, out uint eax, out uint edx, GroupAffinity affinity) {
    GroupAffinity previousAffinity = ThreadAffinity.Set(affinity);
    bool result = ReadMsr(index, out eax, out edx);
    ThreadAffinity.Set(previousAffinity);
    return result;
  }

  /// <summary>
  /// Gets the thermtrip temperature for a specific CPU and core index.
  /// </summary>
  /// <param name="cpuIndex"></param>
  /// <param name="coreIndex"></param>
  /// <returns></returns>
  public uint GetThermtrip(int cpuIndex, uint coreIndex) {
    long[] inArray = new long[2];
    inArray[0] = cpuIndex;
    inArray[1] = coreIndex;
    long[] outArray = _pawnIO.Execute("ioctl_get_thermtrip", inArray, 1);
    return (uint)outArray[0];
  }

  /// <summary>
  /// Closes the PawnIO module and releases any associated resources.
  /// </summary>
  public void Close() => _pawnIO.Close();
}
