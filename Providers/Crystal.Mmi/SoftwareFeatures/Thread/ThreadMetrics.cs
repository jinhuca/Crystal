namespace Crystal.Mmi.SoftwareFeatures.Thread;

public record ThreadMetrics(
  string? Caption,
  string? CreationClassName,
  string? Description,
  ulong? ElapsedTime,           // Total execution runtime millisecond track (uint64)
  uint? ExecutionState,
  string? Handle,               // Unique Thread ID identifier string (Key)
  DateTime? InstallDate,
  ulong? KernelModeTime,        // CPU time spent running kernel instructions (uint64)
  string? LastErrorCode,
  uint? Priority,               // Dynamic base scheduler thread priority ranking
  string? ProcessCreationClassName,
  string? ProcessHandle,        // Owner Process ID (PID) linking back to Win32_Process
  uint? StartAddress,           // Virtual memory entry offset address pointer
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName,
  uint? ThreadState,            // 2 = Running, 5 = Waiting, 6 = Transition
  uint? ThreadWaitReason,       // Schedulers blocking flag identifier index
  ulong? UserModeTime           // CPU time spent running application code (uint64)
) {
  // --- RUNTIME STATE CONVERTERS ---
  // Translates the numeric thread execution state into a plain English status phrase
  public string ThreadStatePhrase => ThreadState switch {
    0 => "Initialized",
    1 => "Ready",
    2 => "Running (Active)",
    3 => "Transition",
    4 => "Terminated",
    5 => "Waiting / Blocked",
    6 => "Transition Space",
    _ => "Unknown State"
  };

  // Translates the numeric thread block reason into an operational diagnosis phrase
  public string WaitReasonPhrase => ThreadWaitReason switch {
    0 or 7 => "Executive / Core Lock",
    1 or 8 => "Free Page Allocation",
    2 or 9 => "Page In Transit",
    3 or 10 => "Pool Allocation Block",
    4 or 11 => "Delay Execution Timer",
    5 or 12 => "Suspended / Frozen State",
    6 or 13 => "User Request Block",
    18 => "Event Pair Delay",
    19 => "LPC Receive Delay",
    20 => "LPC Reply Delay",
    _ => "Not Waiting (Running)"
  };
}
