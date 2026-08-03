using StorageModule.Models;

namespace StorageModule.ViewModels;

/// <summary>One physical drive in the detail list.</summary>
public sealed class StorageDriveViewModel {
  public StorageDriveViewModel(StorageDriveInfo info) {
    Model = info.Model;
    CapacityLabel = info.CapacityGB is { } gb ? $"{gb:0.#} GB" : "—";
    InterfaceType = string.IsNullOrWhiteSpace(info.InterfaceType) ? "—" : info.InterfaceType!;
    MediaType = string.IsNullOrWhiteSpace(info.MediaType) ? "—" : info.MediaType!;
    SerialNumber = string.IsNullOrWhiteSpace(info.SerialNumber) ? "—" : info.SerialNumber!;
    FirmwareRevision = string.IsNullOrWhiteSpace(info.FirmwareRevision) ? "—" : info.FirmwareRevision!;
    PartitionsLabel = info.Partitions is { } p ? p.ToString() : "—";
  }

  public string Model { get; }
  public string CapacityLabel { get; }
  public string InterfaceType { get; }
  public string MediaType { get; }
  public string SerialNumber { get; }
  public string FirmwareRevision { get; }
  public string PartitionsLabel { get; }
}
