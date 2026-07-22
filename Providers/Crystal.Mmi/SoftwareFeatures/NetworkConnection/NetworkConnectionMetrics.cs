namespace Crystal.Mmi.SoftwareFeatures.NetworkConnection;

public record NetworkConnectionMetrics(
  uint? AccessMask,
  string? Caption,
  string? Comment,
  string? ConnectionState,   // "Connected", "Disconnected", "Connecting", etc.
  string? ConnectionType,    // "Current Connection" or "Persistent Connection"
  string? Description,
  string? DisplayType,       // "Domain", "Generic", "Server", "Share"
  DateTime? InstallDate,
  string? LocalName,         // e.g. "Z:"
  string? Name,              // combination of RemoteName and LocalName
  bool? Persistent,
  string? ProviderName,
  string? RemoteName,        // e.g. "\\SERVER"
  string? RemotePath,        // e.g. "\\SERVER\Share"
  string? ResourceType,      // "Disk", "Print", "Any"
  string? Status,
  string? UserName
);
