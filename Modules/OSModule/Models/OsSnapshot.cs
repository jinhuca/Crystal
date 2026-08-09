namespace OSModule.Models;

/// <summary>Static operating-system identity, built once at startup and replayed to every
/// subscriber: edition/version fields, install/boot provenance, and the machine/user identity the
/// detail view lists. Every field is nullable so a source that can't be read collapses to a
/// placeholder rather than an empty string that reads as "known to be blank".</summary>
public sealed record OsSnapshot(
    string? Caption = null,
    string? Edition = null,
    string? Version = null,
    string? BuildNumber = null,
    string? DisplayVersion = null,
    string? Architecture = null,
    string? RegisteredOwner = null,
    string? RegisteredOrganization = null,
    string? MachineName = null,
    string? UserName = null,
    string? SystemDirectory = null,
    string? Locale = null,
    string? TimeZone = null,
    DateTimeOffset? InstallDate = null,
    DateTimeOffset? LastBootTime = null);

/// <summary>A live operating-system reading, re-sampled on a cadence: how long the machine has been
/// up (since <see cref="OsSnapshot.LastBootTime"/>) and the current wall-clock time. Uptime is a
/// <see cref="TimeSpan"/> so the view owns its formatting.</summary>
public sealed record OsLiveReading(
    TimeSpan Uptime,
    DateTimeOffset Now);
