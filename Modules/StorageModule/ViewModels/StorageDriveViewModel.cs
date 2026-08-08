using StorageModule.Models;

namespace StorageModule.ViewModels;

/// <summary>One physical drive in the detail list.</summary>
public sealed class StorageDriveViewModel {
  public StorageDriveViewModel(StorageDriveInfo info) {
    Model = info.Model;
    CapacityLabel = info.CapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    InterfaceType = string.IsNullOrWhiteSpace(info.InterfaceType) ? "—" : info.InterfaceType!;
    MediaType = string.IsNullOrWhiteSpace(info.MediaType) ? "—" : info.MediaType!;
    Manufacturer = string.IsNullOrWhiteSpace(info.Manufacturer) ? "—" : info.Manufacturer!;
    SerialNumber = string.IsNullOrWhiteSpace(info.SerialNumber) ? "—" : info.SerialNumber!;
    FirmwareRevision = string.IsNullOrWhiteSpace(info.FirmwareRevision) ? "—" : info.FirmwareRevision!;
    PartitionsLabel = info.Partitions is { } p ? p.ToString() : "—";

    // Win32_DiskDrive.MediaType reports "Fixed hard disk media" for internal disks, but many
    // NVMe/SSD controllers leave it blank or report "Unknown". Treat a drive as fixed unless it
    // explicitly identifies as removable/external, so those internal drives aren't dropped.
    IsFixedHardDrive = info.MediaType is not { } mt
        || !(mt.Contains("removable", StringComparison.OrdinalIgnoreCase)
             || mt.Contains("external", StringComparison.OrdinalIgnoreCase));

    // WMI's media-type strings ("Fixed hard disk media", …) are too long for the dense tile row;
    // collapse them to a one-word label so the column fits without clipping.
    ShortMediaType =
        info.MediaType is not { } media || string.IsNullOrWhiteSpace(media) ? "Fixed"
        : media.Contains("removable", StringComparison.OrdinalIgnoreCase) ? "Removable"
        : media.Contains("external", StringComparison.OrdinalIgnoreCase) ? "External"
        : media.Contains("fixed", StringComparison.OrdinalIgnoreCase) ? "Fixed"
        : media;
  }

  public string Model { get; }
  public string CapacityLabel { get; }
  public string InterfaceType { get; }
  public string MediaType { get; }
  public string ShortMediaType { get; }
  public string Manufacturer { get; }
  public string SerialNumber { get; }
  public string FirmwareRevision { get; }
  public string PartitionsLabel { get; }
  public bool IsFixedHardDrive { get; }
}
