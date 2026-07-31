using Crystal.Telemetry.Hardware;
using System;

namespace Crystal.Telemetry.PawnIo;

/// <summary>
/// Provides access to the AMD Ryzen System Management Unit (SMU) via the PawnIO driver.
/// </summary>
public class RyzenSmu {
  private readonly PawnIo _pawnIO = PawnIo.LoadModuleFromResource(typeof(IntelMsr).Assembly, $"{nameof(Crystal.Telemetry)}.Resources.PawnIo.RyzenSMU.bin");

  /// <summary>
  /// Reads the SMU firmware version.
  /// </summary>
  /// <returns>The SMU firmware version.</returns>
  public uint GetSmuVersion() {
    if (!Mutexes.WaitPciBus(5000))
      throw new TimeoutException("Timeout waiting for PCI bus mutex");

    uint version;

    try {
      long[] outArray = _pawnIO.Execute("ioctl_get_smu_version", [], 1);
      version = (uint)outArray[0];
    }
    finally {
      Mutexes.ReleasePciBus();
    }

    return version;
  }

  /// <summary>
  /// Reads the processor code name identifier.
  /// </summary>
  /// <returns>The code name identifier.</returns>
  public long GetCodeName() {
    long[] outArray = _pawnIO.Execute("ioctl_get_code_name", [], 1);
    return outArray[0];
  }

  /// <summary>
  /// Reads the power management (PM) table from the SMU.
  /// </summary>
  /// <param name="size">The number of table entries to read.</param>
  /// <returns>The PM table values.</returns>
  public long[] ReadPmTable(int size) {
    if (!Mutexes.WaitPciBus(5000))
      throw new TimeoutException("Timeout waiting for PCI bus mutex");

    try {
      long[] outArray = _pawnIO.Execute("ioctl_read_pm_table", [], size);
      return outArray;
    }
    finally {
      Mutexes.ReleasePciBus();
    }
  }

  /// <summary>
  /// Requests the SMU to refresh the power management (PM) table with current values.
  /// </summary>
  public void UpdatePmTable() {
    if (!Mutexes.WaitPciBus(5000))
      throw new TimeoutException("Timeout waiting for PCI bus mutex");

    try {
      _pawnIO.Execute("ioctl_update_pm_table", [], 0);
    }
    finally {
      Mutexes.ReleasePciBus();
    }
  }

  /// <summary>
  /// Resolves the power management (PM) table version and base address.
  /// </summary>
  /// <param name="version">When this method returns, contains the PM table version.</param>
  /// <param name="tableBase">When this method returns, contains the PM table base address.</param>
  public void ResolvePmTable(out uint version, out uint tableBase) {
    if (!Mutexes.WaitPciBus(5000))
      throw new TimeoutException("Timeout waiting for PCI bus mutex");

    try {
      long[] outArray = _pawnIO.Execute("ioctl_resolve_pm_table", [], 2);
      version = (uint)outArray[0];
      tableBase = (uint)outArray[1];
    }
    finally {
      Mutexes.ReleasePciBus();
    }
  }

  /// <summary>
  /// Closes the underlying PawnIO module.
  /// </summary>
  public void Close() => _pawnIO.Close();
}
