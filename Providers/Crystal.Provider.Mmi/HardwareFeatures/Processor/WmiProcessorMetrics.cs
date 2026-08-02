namespace Crystal.Provider.Mmi.HardwareFeatures.Processor;

/// <summary>
/// Slim, per-socket projection of Win32_Processor used by the CPU inventory
/// pipeline. Correlated to SMBIOS by <see cref="SocketDesignation"/>. This is
/// distinct from the full <see cref="ProcessorMetrics"/> record — the resolver
/// only needs the OS-authoritative counts and the firmware virtualization flag.
/// </summary>
public sealed record WmiProcessorMetrics(
    string? SocketDesignation,
    uint? NumberOfLogicalProcessors,
    uint? NumberOfCores,
    bool? VirtualizationFirmwareEnabled);
