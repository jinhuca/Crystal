namespace Crystal.Mmi.SoftwareFeatures.Share;

public record ShareMetrics(
  uint? AccessMask,      // Deprecated by WMI; typically returns null — use GetAccessMask() instead
  bool? AllowMaximum,
  string? Caption,
  string? Description,
  DateTime? InstallDate,
  uint? MaximumAllowed,   // Only meaningful when AllowMaximum is false
  string? Name,           // Share alias, e.g. "public"
  string? Path,           // Local path being shared, e.g. "C:\Program Files"
  string? Status,
  uint? Type              // 0=Disk, 1=Print Queue, 2=Device, 3=IPC (+0x80000000 for admin shares)
);
