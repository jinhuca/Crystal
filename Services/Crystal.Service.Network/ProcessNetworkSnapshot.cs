namespace Crystal.Service.Network;

/// <summary>
/// One poll of per-process network activity: the top talkers ranked by combined
/// throughput, plus whether the ETW backend is live. When it isn't (<see cref="IsRunning"/> false)
/// the list is empty and <see cref="StatusError"/> explains why (typically "not elevated"), so the
/// view can show a reason instead of a silently blank table.
/// </summary>
/// <param name="TopTalkers">The busiest processes this poll, ranked by combined throughput; empty when the backend is down.</param>
/// <param name="IsRunning">Whether the ETW backend is live and producing rates.</param>
/// <param name="StatusError">Human-readable reason the backend isn't running (e.g. "not elevated"); null when running.</param>
public sealed record ProcessNetworkSnapshot(
    IReadOnlyList<ProcessNetworkReading> TopTalkers,
    bool IsRunning,
    string? StatusError);
