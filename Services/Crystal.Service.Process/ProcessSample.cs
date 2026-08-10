namespace Crystal.Service.Process;

/// <summary>
/// One process's readings for a single poll. CPU is a percentage of total machine capacity
/// (0-100 across all logical cores, like Task Manager's default). Memory is working-set MB.
/// The GPU/Disk/Network fields are null until the ETW backend supplies them — the row shows
/// a placeholder for a null value rather than a misleading zero.
/// </summary>
public sealed record ProcessSample(
    uint ProcessId,
    string Name,
    double CpuPercent,
    double WorkingSetMb,
    ProcessCategory Category,
    string? Status = null,
    double? GpuPercent = null,
    double? DiskBytesPerSec = null,
    double? NetBytesPerSec = null,
    string? ExecutablePath = null);
