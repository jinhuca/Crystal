namespace Crystal.Provider.Mmi.HardwareFeatures.VideoController;

public record VideoControllerMetrics(
  ushort? Availability,
  string? AdapterCompatibility,
  string? AdapterDACType,
  uint? AdapterRAM,             // VRAM size in bytes (reported up to uint.MaxValue)
  ushort? Architecture,
  string? Caption,
  uint? ColorTableEntries,
  uint? ConfigManagerErrorCode,
  bool? ConfigManagerUserConfig,
  string? CreationClassName,
  uint? CurrentBitsPerPixel,
  uint? CurrentHorizontalResolution, // e.g., 1920
  ulong? CurrentNumberOfColors,
  uint? CurrentNumberOfColumns,
  uint? CurrentNumberOfRows,
  uint? CurrentRefreshRate,         // Screen refresh frequency in Hz (e.g., 60, 144)
  uint? CurrentVerticalResolution,   // e.g., 1080
  string? Description,
  string? DeviceID,
  uint? DitherType,
  bool? ErrorCleared,
  string? ErrorDescription,
  uint? ICMIntent,
  uint? ICMMethod,
  DateTime? InfDate,
  string? InfSection,
  string? InstalledDisplayDrivers,   // Path or file strings for driver sets
  string? DriverVersion,             // Active display driver version string
  DateTime? InstallDate,
  uint? LastErrorCode,
  uint? MaxMemorySupported,
  uint? MaxRefreshRate,
  uint? MinRefreshRate,
  string? Name,                      // GPU Product Name (e.g., "NVIDIA GeForce RTX 4070")
  ushort? VideoArchitecture,         // 5 = VGA, 10 = PCI, etc.
  ushort? VideoMemoryType,
  string? VideoProcessor,            // The actual GPU chip name
  string? PNPDeviceID,
  ushort[]? PowerManagementCapabilities,
  bool? PowerManagementSupported,
  string? Status,
  ushort? StatusInfo,
  string? SystemCreationClassName,
  string? SystemName
) {
  // --- RUNTIME PRESENTATION HELPERS ---

  // Computes Video RAM into a clean, human-readable Gigabytes format
  public double? VideoRamInGB => AdapterRAM.HasValue
    ? Math.Round((double)AdapterRAM.Value / 1024.0 / 1024.0 / 1024.0, 2)
    : null;

  // Returns a consolidated resolution layout string (e.g., "1920 x 1080 @ 144Hz")
  public string FormattedDisplayMode => (CurrentHorizontalResolution.HasValue && CurrentVerticalResolution.HasValue)
    ? $"{CurrentHorizontalResolution.Value} x {CurrentVerticalResolution.Value} @ {CurrentRefreshRate ?? 0}Hz ({CurrentBitsPerPixel ?? 0}-bit color)"
    : "No Monitor Active / Headless Display Mode";
}
