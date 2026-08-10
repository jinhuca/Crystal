namespace Crystal.Service.Process;

/// <summary>System-wide totals shown in the process summary header: the number of running
/// processes and the sum of their threads and open handles. A single snapshot taken once per
/// poll.</summary>
public readonly record struct SystemStats(int Processes, int Threads, int Handles);
