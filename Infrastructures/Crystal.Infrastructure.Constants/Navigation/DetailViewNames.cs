namespace Crystal.Infrastructure.Constants.Navigation;

/// <summary>
/// Navigation names each module registers its full-scale detail view under, and that
/// summary tiles pass as the <see cref="ShowDetailEvent"/> payload. Keeping them here
/// lets the shell and modules agree on a name without a project reference between them.
/// </summary>
public static class DetailViewNames {
  public const string Cpu = "CpuDetailView";
  public const string Gpu = "GpuDetailView";
  public const string Memory = "MemoryDetailView";
  public const string Storage = "StorageDetailView";
  public const string Bios = "BiosDetailView";
  public const string Network = "NetworkDetailView";
  public const string Os = "OsDetailView";

  // Opens the full process list (ProcessDetailView) in a detail window, raised by the compact
  // Processes tile's double-click. The value doubles as the window title.
  public const string Process = "Processes";

  // Opens the benchmark suite (BenchmarkDetailView) in a detail window, raised by the shell
  // title-bar and Processes-toolbar Benchmark buttons. The value doubles as the window title.
  public const string Benchmark = "Benchmark";
}
