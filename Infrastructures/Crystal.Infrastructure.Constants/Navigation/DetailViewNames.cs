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
}
