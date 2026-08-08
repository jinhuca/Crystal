namespace CpuModule.Models;

/// <summary>System-wide totals shown in the summary footer: the number of running processes and
/// the sum of their threads and open handles. A single snapshot taken once per poll.</summary>
public readonly record struct SystemStats(int Processes, int Threads, int Handles);
