using System.Collections.Frozen;
using Crystal.Mmi.MmiEngine;

namespace Crystal.Mmi.Tests.Helpers;

/// <summary>
/// A simple in-memory stub for IWmiHardwareProvider that returns pre-configured data per WMI class name.
/// No mocking library needed — the interface has a single method.
/// </summary>
internal sealed class FakeWmiProvider : IWmiHardwareProvider
{
    private readonly Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>> _data;

    public FakeWmiProvider(Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>> data)
    {
        _data = new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>(
            data, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Convenience constructor for a single-class provider.</summary>
    public FakeWmiProvider(string className, IReadOnlyList<FrozenDictionary<string, WmiValue>> rows)
        : this(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            [className] = rows
        })
    { }

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string wmiClassName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_data.TryGetValue(wmiClassName, out var result))
            return Task.FromResult(result);
        return Task.FromResult<IReadOnlyList<FrozenDictionary<string, WmiValue>>>(
            Array.Empty<FrozenDictionary<string, WmiValue>>());
    }
}

/// <summary>Static factory helpers to build WMI data rows concisely in tests.</summary>
internal static class WmiRow
{
    public static FrozenDictionary<string, WmiValue> Build(params (string Key, WmiValue Value)[] entries)
        => entries.ToDictionary(e => e.Key, e => e.Value).ToFrozenDictionary();

    public static IReadOnlyList<FrozenDictionary<string, WmiValue>> Single(
        params (string Key, WmiValue Value)[] entries)
        => new[] { Build(entries) };

    public static IReadOnlyList<FrozenDictionary<string, WmiValue>> Many(
        IEnumerable<FrozenDictionary<string, WmiValue>> rows)
        => rows.ToList();

    public static IReadOnlyList<FrozenDictionary<string, WmiValue>> Empty()
        => Array.Empty<FrozenDictionary<string, WmiValue>>();
}
