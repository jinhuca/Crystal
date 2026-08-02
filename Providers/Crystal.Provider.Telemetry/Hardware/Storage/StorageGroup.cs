using DiskInfoToolkit;
using System.Collections.Generic;
using System.Linq;
using StorageDIT = DiskInfoToolkit.Storage;

namespace Crystal.Provider.Telemetry.Hardware.Storage;

internal class StorageGroup : IGroup, IHardwareChanged {
  private readonly List<StorageDevice> _hardware = new();

  private readonly ISettings _settings;

  public event HardwareEventHandler HardwareAdded;
  public event HardwareEventHandler HardwareRemoved;

  public StorageGroup(ISettings settings) {
    if (Software.OperatingSystem.IsUnix)
      return;

    _settings = settings;

    AddHardware(settings);
  }

  public IReadOnlyList<IHardware> Hardware => _hardware;

  private void AddHardware(ISettings settings) {
    StorageDIT.DevicesChanged -= OnStoragesChanged;

    //Get all disks
    var disks = StorageDIT.GetDisks();

    //Transform storage device to hardware
    _hardware.AddRange(disks.Select(s => new StorageDevice(s, settings)));

    StorageDIT.DevicesChanged += OnStoragesChanged;
  }

  private void OnStoragesChanged(object sender, StorageDevicesChangedEventArgs e) {
    foreach (var added in e.Added) {
      var storageDevice = new StorageDevice(added, _settings);

      _hardware.Add(storageDevice);
      HardwareAdded?.Invoke(storageDevice);
    }

    foreach (var removed in e.Removed) {
      var storageDevice = _hardware.Find(sd => sd.Storage == removed);
      if (storageDevice != null) {
        _hardware.Remove(storageDevice);
        HardwareRemoved?.Invoke(storageDevice);
      }
    }
  }

  public void Close() { }

  public string GetReport() => null;
}
