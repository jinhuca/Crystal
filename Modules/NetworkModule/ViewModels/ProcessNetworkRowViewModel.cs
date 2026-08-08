using NetworkModule.Models;

namespace NetworkModule.ViewModels;

/// <summary>One row in the network detail view's top-talkers table: a process's name/PID and its
/// current combined network throughput, formatted for display. Rows are reconciled in place by PID
/// so the table doesn't flicker between polls.</summary>
public sealed class ProcessNetworkRowViewModel : BindableBase {
  private string _name = "—";
  private string _rateLabel = "—";
  private double _rateBytesPerSecond;

  public ProcessNetworkRowViewModel(uint processId) => ProcessId = processId;

  public uint ProcessId { get; }
  public string Name { get => _name; private set => SetProperty(ref _name, value); }
  public string RateLabel { get => _rateLabel; private set => SetProperty(ref _rateLabel, value); }

  /// <summary>Raw rate, kept so the collection can sort rows without reparsing the label.</summary>
  public double RateBytesPerSecond { get => _rateBytesPerSecond; private set => SetProperty(ref _rateBytesPerSecond, value); }

  public void Update(ProcessNetworkReading reading) {
    Name = reading.Name;
    RateBytesPerSecond = reading.NetBytesPerSecond;
    RateLabel = FormatSpeed(reading.NetBytesPerSecond);
  }

  private static string FormatSpeed(double bytesPerSecond) {
    if (bytesPerSecond >= 1024d * 1024 * 1024) return $"{bytesPerSecond / (1024d * 1024 * 1024):0.00} GiB/s";
    if (bytesPerSecond >= 1024d * 1024) return $"{bytesPerSecond / (1024d * 1024):0.00} MiB/s";
    if (bytesPerSecond >= 1024d) return $"{bytesPerSecond / 1024d:0.00} KiB/s";
    return $"{bytesPerSecond:0} B/s";
  }
}
