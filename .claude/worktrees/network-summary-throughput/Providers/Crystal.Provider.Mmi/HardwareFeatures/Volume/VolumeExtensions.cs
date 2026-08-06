namespace Crystal.Provider.Mmi.HardwareFeatures.Volume;

public static class VolumeExtensions
{
    private const double Gb = 1024d * 1024d * 1024d;
    private const double Tb = Gb * 1024d;

    public static double? CapacityInTB(this VolumeMetrics volume) => volume.Capacity is null ? null : Math.Round(volume.Capacity.Value / Tb, 2);
    public static double? UsedSpaceInGB(this VolumeMetrics volume) => volume.Capacity is null || volume.FreeSpace is null ? null : Math.Round((volume.Capacity.Value - volume.FreeSpace.Value) / Gb, 2);
    public static bool IsHealthy(this VolumeMetrics volume) => string.Equals(volume.Status, "OK", StringComparison.OrdinalIgnoreCase);
    public static bool IsLowDiskSpace(this VolumeMetrics volume, double thresholdPercent = 10) => volume.FreePercent is not null && volume.FreePercent <= thresholdPercent;
    public static bool IsSystemOrBootVolume(this VolumeMetrics volume) => volume.SystemVolume == true || volume.BootVolume == true;
    public static string CapacitySummary(this VolumeMetrics volume) => volume.CapacityInGB is null || volume.FreeSpaceInGB is null ? "Unknown" : $"{volume.FreeSpaceInGB:F2} GB free of {volume.CapacityInGB:F2} GB";
}
