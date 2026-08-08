namespace NetworkModule.Models;

/// <summary>One poll of per-process network activity: the top talkers ranked by combined
/// throughput, plus whether the ETW backend is live. When it isn't (<see cref="IsRunning"/> false)
/// the list is empty and <see cref="StatusError"/> explains why (typically "not elevated"), so the
/// view can show a reason instead of a silently blank table.</summary>
public sealed record ProcessNetworkSnapshot(
    IReadOnlyList<ProcessNetworkReading> TopTalkers,
    bool IsRunning,
    string? StatusError);
