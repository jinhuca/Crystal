namespace Crystal.Provider.Mmi.PerformanceFeatures.PerfCounter;

/// <summary>
/// Metrics record for WMI class <c>Win32_Perf</c>.
/// Abstract base class for all WMI performance counter classes.
/// Contains the timing-header fields shared by every perf counter subclass.
/// </summary>
public record PerfCounterMetrics(
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
