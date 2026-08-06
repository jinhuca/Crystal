namespace Crystal.Provider.CpuId;

/// <summary>
/// Queries the executing processor via the CPUID instruction and returns a
/// decoded snapshot. Implementations are synchronous — CPUID is a register-level
/// instruction with no I/O.
/// </summary>
public interface ICpuIdProvider {
  CpuIdRawData Query();
}
