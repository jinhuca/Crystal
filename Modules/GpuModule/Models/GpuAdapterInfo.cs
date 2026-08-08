namespace GpuModule.Models;

/// <summary>Whether an adapter is the CPU's integrated graphics or a discrete card. Used to
/// label the two columns in the reference design ("Integrated GPU" / "Dedicated GPU").</summary>
public enum GpuKind { Integrated, Dedicated }

/// <summary>
/// Static per-adapter GPU inventory read once from WMI (<c>Win32_VideoController</c>):
/// product name, VRAM, current display mode and driver. Live load is carried separately
/// on the sensor stream and matched back to this adapter by <see cref="Name"/>.
/// </summary>
public sealed record GpuAdapterInfo(
    string Name,
    GpuKind Kind,
    double? VideoRamGB,
    string DisplayMode,
    string? DriverVersion,
    DateTime? DriverDate,
    string? VideoProcessor,
    string? PhysicalLocation,
    uint? RefreshRateHz);
