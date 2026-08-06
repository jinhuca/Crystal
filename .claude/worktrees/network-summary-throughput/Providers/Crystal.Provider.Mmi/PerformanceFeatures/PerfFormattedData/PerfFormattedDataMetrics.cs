namespace Crystal.Provider.Mmi.PerformanceFeatures.PerfFormattedData;

/// <summary>
/// Metrics record for WMI class <c>Win32_PerfFormattedData</c>.
/// Abstract base class for calculated (post-processed) performance counters — the
/// counterpart to <c>Win32_PerfRawData</c>, which exposes the raw, uncalculated values.
/// Inherits all timing-header fields from <c>Win32_Perf</c> and adds no further
/// base-level properties; concrete subclasses (e.g. Win32_PerfFormattedData_PerfOS_Processor)
/// append their own calculated counters.
/// </summary>
public record PerfFormattedDataMetrics(
    string? Caption,
    string? Description,
    string? Name,
    ulong? Frequency_Object,
    ulong? Frequency_PerfTime,
    ulong? Frequency_Sys100NS,
    ulong? Timestamp_Object,
    ulong? Timestamp_PerfTime,
    ulong? Timestamp_Sys100NS
);
