namespace Crystal.Provider.Mmi.PerformanceFeatures.PerfRawData;

/// <summary>
/// Metrics record for WMI class <c>Win32_PerfRawData</c>.
/// Abstract base class for raw (non-computed) performance counters.
/// Inherits all timing-header fields from <c>Win32_Perf</c> and adds no
/// further base-level properties; concrete subclasses append their own counters.
/// </summary>
public record PerfRawDataMetrics(
    string? Caption,
    string? Description,
    string? Name,
    ulong?  Frequency_Object,
    ulong?  Frequency_PerfTime,
    ulong?  Frequency_Sys100NS,
    ulong?  Timestamp_Object,
    ulong?  Timestamp_PerfTime,
    ulong?  Timestamp_Sys100NS
);
