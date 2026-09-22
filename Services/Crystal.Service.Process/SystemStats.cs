namespace Crystal.Service.Process;

/// <summary>
/// System-wide totals shown in the process summary header and the detail-panel utilization
/// graphs: the number of running processes and the summed thread and open-handle counts, plus true
/// machine-wide CPU and memory utilization. <see cref="CpuPercent"/> is whole-machine busy time
/// (0-100 across all logical cores, like Task Manager) from GetSystemTimes — not a sum of
/// per-process CPU, which double-counts the idle process. <see cref="MemoryPercent"/> is the OS
/// memory load (used physical / total). A single snapshot taken once per poll.
/// </summary>
/// <param name="Processes">Number of running processes at the moment of the snapshot.</param>
/// <param name="Threads">Total thread count summed across every running process.</param>
/// <param name="Handles">Total open-handle count summed across every running process.</param>
/// <param name="CpuPercent">Whole-machine CPU busy fraction (0-100) from GetSystemTimes.</param>
/// <param name="MemoryPercent">OS memory load percentage (used physical / total).</param>
/// <param name="MemoryTotalMb">Total installed physical memory in megabytes.</param>
public readonly record struct SystemStats(
    int Processes,
    int Threads,
    int Handles,
    double CpuPercent,
    double MemoryPercent,
    double MemoryTotalMb);
