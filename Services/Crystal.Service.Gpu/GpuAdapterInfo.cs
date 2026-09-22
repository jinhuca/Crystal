namespace Crystal.Service.Gpu;

/// <summary>
/// Whether an adapter is the CPU's integrated graphics or a discrete card. Used to
/// label the two columns in the reference design ("Integrated GPU" / "Dedicated GPU").
/// </summary>
public enum GpuKind { Integrated, Dedicated }

/// <summary>
/// Static per-adapter GPU inventory read once from WMI (<c>Win32_VideoController</c>):
/// product name, VRAM, current display mode and driver. Live load is carried separately
/// on the sensor stream and matched back to this adapter by <see cref="Name"/>.
/// </summary>
/// <param name="Name">The adapter's product name as reported by WMI; also the key used to
/// match this row against a live <see cref="GpuLoadReading"/>.</param>
/// <param name="Kind">Whether this adapter is integrated graphics or a discrete card, used to
/// place it in the correct column of the reference design.</param>
/// <param name="VideoRamGB">Dedicated video memory in GB, or null when WMI reports none.</param>
/// <param name="DisplayMode">The current display mode string (resolution, colour depth), preformatted for display.</param>
/// <param name="DriverVersion">The installed display-driver version, or null when unknown.</param>
/// <param name="DriverDate">The driver's release date, resolved from the registry class key with a
/// WMI fallback (see <see cref="GpuInfoBuilder"/>); null when it cannot be determined.</param>
/// <param name="VideoProcessor">The GPU chip/processor name as reported by WMI, or null when unknown.</param>
/// <param name="PhysicalLocation">The human-readable PCI location ("PCI bus X, device Y, function Z"),
/// parsed from the device's registry LocationInformation; null when unavailable.</param>
/// <param name="RefreshRateHz">The current display refresh rate in Hz, or null when WMI reports none.</param>
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
