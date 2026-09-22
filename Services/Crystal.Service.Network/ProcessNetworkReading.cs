namespace Crystal.Service.Network;

/// <summary>
/// One process's combined (send + receive) network throughput for a single poll, used to
/// rank the network detail view's top-talkers table. <see cref="NetBytesPerSecond"/> is the ETW
/// per-process rate over the window since the previous snapshot.
/// </summary>
/// <param name="ProcessId">The owning process's PID.</param>
/// <param name="Name">Resolved process name, or a "PID {id}" fallback when the process has exited.</param>
/// <param name="NetBytesPerSecond">Combined send + receive throughput in bytes/second over the poll window.</param>
public sealed record ProcessNetworkReading(
    uint ProcessId,
    string Name,
    double NetBytesPerSecond);
