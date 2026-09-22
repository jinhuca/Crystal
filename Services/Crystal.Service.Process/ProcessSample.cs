namespace Crystal.Service.Process;

/// <summary>
/// One process's readings for a single poll. CPU is a percentage of total machine capacity
/// (0-100 across all logical cores, like Task Manager's default). Memory is working-set MB.
/// The GPU/Disk/Network fields are null until the ETW backend supplies them — the row shows
/// a placeholder for a null value rather than a misleading zero.
/// </summary>
/// <param name="ProcessId">The OS process identifier (PID) the reading belongs to.</param>
/// <param name="Name">The process image name, e.g. <c>chrome.exe</c>.</param>
/// <param name="CpuPercent">Whole-machine CPU busy fraction (0-100 across all logical cores).</param>
/// <param name="WorkingSetMb">Working-set (physical) memory in megabytes.</param>
/// <param name="Category">Whether the process is an App, background, or Windows/system process.</param>
/// <param name="Status">The process status string; typically "Running".</param>
/// <param name="GpuPercent">GPU utilization (0-100), or null until an ETW/counter source supplies it.</param>
/// <param name="DiskBytesPerSec">Disk throughput in bytes/second, or null when no source has supplied it.</param>
/// <param name="NetBytesPerSec">Network throughput in bytes/second, or null when no source has supplied it.</param>
/// <param name="ExecutablePath">Full path to the on-disk image (used for the shell icon), or null when unreadable.</param>
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
