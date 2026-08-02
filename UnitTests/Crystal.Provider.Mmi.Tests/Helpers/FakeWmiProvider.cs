using System.Collections.Frozen;
using Crystal.Provider.Mmi.MmiEngine;

namespace Crystal.Provider.Mmi.Tests.Helpers;

/// <summary>
/// A simple in-memory stub for IWmiHardwareProvider that returns pre-configured data per WMI class name.
/// No mocking library needed — the interface methods are backed by dictionaries.
/// </summary>
internal sealed class FakeWmiProvider : IWmiHardwareProvider
{
    private readonly Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>> _data;
    private readonly Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>> _queries;
    private readonly Dictionary<string, WmiMethodResult> _methods;

    public FakeWmiProvider(Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>> data)
    {
        _data = new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>(
            data, StringComparer.OrdinalIgnoreCase);
        _queries = new(StringComparer.OrdinalIgnoreCase);
        _methods = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Convenience constructor for a single-class provider.</summary>
    public FakeWmiProvider(string className, IReadOnlyList<FrozenDictionary<string, WmiValue>> rows)
        : this(new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>
        {
            [className] = rows
        })
    { }

    /// <summary>Registers rows to return for an exact WQL query string (used by <see cref="QueryAsync"/>).</summary>
    public FakeWmiProvider WithQuery(string wqlQuery, IReadOnlyList<FrozenDictionary<string, WmiValue>> rows)
    {
        _queries[wqlQuery] = rows;
        return this;
    }

    /// <summary>Registers a result to return for an invoked method, keyed as <c>Class.Method</c>.</summary>
    public FakeWmiProvider WithMethod(string className, string methodName, WmiMethodResult result)
    {
        _methods[$"{className}.{methodName}"] = result;
        return this;
    }

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string wmiClassName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_data.TryGetValue(wmiClassName, out var result))
            return Task.FromResult(result);
        return Task.FromResult<IReadOnlyList<FrozenDictionary<string, WmiValue>>>(
            Array.Empty<FrozenDictionary<string, WmiValue>>());
    }

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> GetMultiMetricsForClassAsync(
        string namespaceName, string wmiClassName, CancellationToken cancellationToken)
        // Namespace is ignored by the fake — the class name is the lookup key.
        => GetMultiMetricsForClassAsync(wmiClassName, cancellationToken);

    public Task<IReadOnlyList<FrozenDictionary<string, WmiValue>>> QueryAsync(
        string namespaceName, string wqlQuery, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_queries.TryGetValue(wqlQuery, out var result))
            return Task.FromResult(result);
        return Task.FromResult<IReadOnlyList<FrozenDictionary<string, WmiValue>>>(
            Array.Empty<FrozenDictionary<string, WmiValue>>());
    }

    public Task<WmiMethodResult> InvokeStaticMethodAsync(
        string namespaceName, string wmiClassName, string methodName,
        IReadOnlyDictionary<string, WmiValue> inParameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_methods.TryGetValue($"{wmiClassName}.{methodName}", out var result))
            return Task.FromResult(result);
        return Task.FromResult(WmiMethodResult.Empty);
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
