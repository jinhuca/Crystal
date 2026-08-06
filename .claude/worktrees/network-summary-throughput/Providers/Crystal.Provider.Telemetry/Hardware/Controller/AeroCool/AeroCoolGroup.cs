using HidSharp;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Provider.Telemetry.Hardware.Controller.AeroCool;

/// <summary>
/// <see cref="IGroup" /> containing all detected AeroCool <see cref="IHardware" />.
/// </summary>
public class AeroCoolGroup : IGroup {
  private readonly List<IHardware> _hardware = new();
  private readonly StringBuilder _report = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="AeroCoolGroup" /> class and detects supported AeroCool devices.
  /// </summary>
  /// <param name="settings">Additional settings passed to each detected <see cref="IHardware" />.</param>
  public AeroCoolGroup(ISettings settings) {
    _report.AppendLine("AeroCool Hardware");
    _report.AppendLine();

    foreach (HidDevice dev in DeviceList.Local.GetHidDevices(0x2E97)) {
      int hubno = dev.ProductID - 0x1000;
      if (dev.DevicePath.Contains("mi_02") && hubno is >= 1 and <= 8) {
        var device = new P7H1(dev, settings);
        _report.AppendLine($"Device name: {device.Name}");
        _report.AppendLine($"HUB number: {device.HubNumber}");
        _report.AppendLine();
        _hardware.Add(device);
      }
    }

    if (_hardware.Count == 0) {
      _report.AppendLine("No AeroCool Hardware found.");
      _report.AppendLine();
    }
  }

  /// <inheritdoc />
  public IReadOnlyList<IHardware> Hardware => _hardware;

  /// <inheritdoc />
  public void Close() {
    foreach (IHardware iHardware in _hardware) {
      if (iHardware is Hardware hardware)
        hardware.Close();
    }
  }

  /// <inheritdoc />
  public string GetReport() {
    return _report.ToString();
  }
}