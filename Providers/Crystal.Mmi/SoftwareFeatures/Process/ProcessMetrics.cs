namespace Crystal.Mmi.SoftwareFeatures.Process;

public record ProcessMetrics(
  string? Caption,
  string? CommandLine,         // The full execution string (e.g., "chrome.exe --type=renderer")
  string? CreationClassName,
  DateTime? CreationDate,      // Process startup timestamp
  string? Description,
  string? ExecutablePath,      // Full path to the executable file on disk
  uint? ExecutionState,
  uint? Handle,                // Process ID (PID) used across the OS (Key)
  uint? HandleCount,           // Total opened kernel handles
  DateTime? InstallDate,
  ulong? KernelModeTime,       // CPU time spent running kernel instructions (uint64)
  uint? MaximumWorkingSetSize,
  uint? MinimumWorkingSetSize,
  string? Name,                // Executable name (e.g., "explorer.exe")
  ulong? OtherOperationCount,
  ulong? OtherTransferCount,
  uint? PageFaults,
  uint? PageFileUsage,
  uint? ParentProcessId,       // Parent Process ID (PID) that launched this process
  uint? PeakPageFileUsage,
  ulong? PeakVirtualSize,
  ulong? PeakWorkingSetSize,
  uint? Priority,              // Base thread priority scheduling rank
  ulong? PrivatePageCount,
  uint? ProcessId,             // Duplicate of Handle for explicit mapping
  ulong? ReadOperationCount,
  ulong? ReadTransferCount,
  uint? SessionId,             // Windows terminal session isolation ID
  string? Status,
  DateTime? TerminationDate,
  ulong? UserModeTime,         // CPU time spent running user application code (uint64)
  ulong? VirtualSize,          // Virtual memory size in bytes (uint64)
  string? WindowsVersion,
  ulong? WorkingSetSize,       // Actual RAM consumed in bytes (uint64)
  ulong? WriteOperationCount,
  ulong? WriteTransferCount
) {
  // --- RUNTIME MEMORY CONVERTERS ---

  // Computes working set RAM consumption into a clean Megabytes presentation format
  public double? WorkingSetInMB => WorkingSetSize.HasValue
    ? Math.Round(WorkingSetSize.Value / 1024.0 / 1024.0, 2)
    : null;

  // Computes virtual memory footprint into a clean Megabytes presentation format
  public double? VirtualSizeInMB => VirtualSize.HasValue
    ? Math.Round(VirtualSize.Value / 1024.0 / 1024.0, 2)
    : null;
}

