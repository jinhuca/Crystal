namespace Crystal.Service.Process;

/// <summary>System-wide totals shown in the process summary header and the detail-panel utilization
/// graphs: the number of running processes and the summed thread and open-handle counts, plus true
/// machine-wide CPU and memory utilization. <see cref="CpuPercent"/> is whole-machine busy time
/// (0-100 across all logical cores, like Task Manager) from GetSystemTimes — not a sum of
/// per-process CPU, which double-counts the idle process. <see cref="MemoryPercent"/> is the OS
/// memory load (used physical / total). A single snapshot taken once per poll.</summary>
public readonly record struct SystemStats(
    int Processes,
    int Threads,
    int Handles,
    double CpuPercent,
    double MemoryPercent,
    double MemoryTotalMb);
