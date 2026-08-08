namespace NetworkModule.Models;

/// <summary>One process's combined (send + receive) network throughput for a single poll, used to
/// rank the network detail view's top-talkers table. <see cref="NetBytesPerSecond"/> is the ETW
/// per-process rate over the window since the previous snapshot.</summary>
public sealed record ProcessNetworkReading(
    uint ProcessId,
    string Name,
    double NetBytesPerSecond);
