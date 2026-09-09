using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Crystal.Controls.Metrics;

/// <summary>
/// A single live metric with its current value, session min/max/avg, and a short-term trend, in the
/// consumer's unit. Updated in place on every sensor emission via <see cref="Update"/>; a reading the
/// platform doesn't expose stays at zero. The avg and trend recover the range/direction cues a
/// sparkline conveyed, computed from the sample stream at effectively no render cost.
/// <para>
/// Implements <see cref="INotifyPropertyChanged"/> directly (rather than a framework base) so the UI
/// assembly stays free of an MVVM-framework dependency and every module can reuse the type.
/// </para>
/// </summary>
public sealed class MetricRowViewModel : INotifyPropertyChanged {
  /// <summary>Weight of each new sample in the trend baseline; ~0.3 smooths 1 Hz jitter without lag.</summary>
  private const double EmaAlpha = 0.3;

  /// <summary>Trend deadband as a fraction of the baseline: movement under 1% reads as flat.</summary>
  private const double TrendBand = 0.01;

  private double _value;
  private double _min;
  private double _max;
  private double _avg;
  private MetricTrend _trend;

  private double _sum;
  private long _count;
  private double _ema;
  private bool _seeded;

  public MetricRowViewModel(string label) => Label = label;

  public event PropertyChangedEventHandler? PropertyChanged;

  /// <summary>The row label (e.g. Package / Cores, Core / Effective, 3D / Clock / HotSpot).</summary>
  public string Label { get; }

  // NOTE: setters are public because consumers bind Run.Text/TextBlock.Text to these; WPF attaches
  // those bindings writably and a read-only property makes it throw at load. Update() is the intended
  // sole writer, so the running avg/trend stay consistent regardless.

  /// <summary>This row's current reading, in the consumer's unit.</summary>
  public double Value { get => _value; set => SetProperty(ref _value, value); }

  /// <summary>This row's session-minimum reading, in the consumer's unit.</summary>
  public double Min { get => _min; set => SetProperty(ref _min, value); }

  /// <summary>This row's session-maximum reading, in the consumer's unit.</summary>
  public double Max { get => _max; set => SetProperty(ref _max, value); }

  /// <summary>This row's running session mean, in the consumer's unit.</summary>
  public double Avg { get => _avg; set => SetProperty(ref _avg, value); }

  /// <summary>Short-term direction of the reading versus its smoothed baseline.</summary>
  public MetricTrend Trend { get => _trend; set => SetProperty(ref _trend, value); }

  /// <summary>
  /// Folds a new reading into the row: sets the current value, updates the session min/max/avg and
  /// the trend. When <paramref name="min"/>/<paramref name="max"/> are supplied (the provider's own
  /// session extremes) they are used verbatim; otherwise the row tracks its own extremes from the
  /// values it has seen (e.g. a fan, which has no provider min/max). Trend compares the new value
  /// against the pre-update EMA baseline so a single-tick spike can't both move the baseline and be
  /// judged against it.
  /// </summary>
  public void Update(double value, double? min = null, double? max = null) {
    if (_seeded) {
      double band = Math.Abs(_ema) * TrendBand;
      Trend = value > _ema + band ? MetricTrend.Rising
            : value < _ema - band ? MetricTrend.Falling
            : MetricTrend.Flat;
      _ema += EmaAlpha * (value - _ema);
    } else {
      _ema = value;
      _seeded = true;
      Trend = MetricTrend.Flat;
    }

    _sum += value;
    _count++;
    Avg = _sum / _count;

    Value = value;
    Min = min ?? (_count == 1 ? value : Math.Min(_min, value));
    Max = max ?? (_count == 1 ? value : Math.Max(_max, value));
  }

  private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null) {
    if (Equals(field, value)) return;
    field = value;
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}
