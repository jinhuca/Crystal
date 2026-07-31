using HidSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crystal.Telemetry.Hardware.Psu.Msi;

/// <summary>
/// <see cref="IGroup" /> containing all detected MSI Ai series PSU <see cref="IHardware" />.
/// </summary>
public class MsiPsuGroup : IGroup {
  private static readonly int[] _productIds =
  {
        0x56d4, // MEG Ai1300P
    };

  private static readonly ushort _vendorId = 0x0db0;
  private readonly List<IHardware> _hardware;
  private readonly StringBuilder _report;

  /// <summary>
  /// Initializes a new instance of the <see cref="MsiPsuGroup" /> class and detects supported MSI PSU devices.
  /// </summary>
  /// <param name="settings">Additional settings passed to each detected <see cref="IHardware" />.</param>
  public MsiPsuGroup(ISettings settings) {
    _report = new StringBuilder();
    _report.AppendLine("MSI Ai series PSU Hardware");
    _report.AppendLine();

    _hardware = new List<IHardware>();
    foreach (HidDevice dev in DeviceList.Local.GetHidDevices(_vendorId)) {
      if (_productIds.Contains(dev.ProductID)) {
        var device = new MsiPsu(dev, settings, _hardware.Count);
        _hardware.Add(device);
        _report.AppendLine($"Device name: {device.Name}");
        _report.AppendLine();
      }
    }

    if (_hardware.Count == 0) {
      _report.AppendLine("No MSI PSU Hardware found.");
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
