using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.RangeBars;

/// <summary>
/// A single reference marker drawn on a <see cref="RangeBar"/> at a point on its
/// <see cref="RangeBar.MinValue"/>..<see cref="RangeBar.MaxValue"/> scale — a limit line, a session
/// peak, a target, etc. The control converts <see cref="Value"/> to a pixel position itself, so
/// consumers never hardcode offsets. A <see cref="Label"/> is optional; when several labels would
/// collide, give them different <see cref="LabelLine"/> values to stack them vertically.
/// <para>Derives from <see cref="Freezable"/> so its properties are bindable/animatable and a live
/// change re-renders the owning bar.</para>
/// </summary>
public class RangeBarMarker : Freezable {
  /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
  public static readonly DependencyProperty ValueProperty =
      DependencyProperty.Register(nameof(Value), typeof(double), typeof(RangeBarMarker), new PropertyMetadata(0.0));

  /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
  public static readonly DependencyProperty BrushProperty =
      DependencyProperty.Register(nameof(Brush), typeof(Brush), typeof(RangeBarMarker), new PropertyMetadata(Brushes.Gray));

  /// <summary>Identifies the <see cref="Thickness"/> dependency property.</summary>
  public static readonly DependencyProperty ThicknessProperty =
      DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(RangeBarMarker), new PropertyMetadata(2.0));

  /// <summary>Identifies the <see cref="Overhang"/> dependency property.</summary>
  public static readonly DependencyProperty OverhangProperty =
      DependencyProperty.Register(nameof(Overhang), typeof(double), typeof(RangeBarMarker), new PropertyMetadata(0.0));

  /// <summary>Identifies the <see cref="Label"/> dependency property.</summary>
  public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register(nameof(Label), typeof(string), typeof(RangeBarMarker), new PropertyMetadata(null));

  /// <summary>Identifies the <see cref="LabelBrush"/> dependency property.</summary>
  public static readonly DependencyProperty LabelBrushProperty =
      DependencyProperty.Register(nameof(LabelBrush), typeof(Brush), typeof(RangeBarMarker), new PropertyMetadata(null));

  /// <summary>Identifies the <see cref="LabelLine"/> dependency property.</summary>
  public static readonly DependencyProperty LabelLineProperty =
      DependencyProperty.Register(nameof(LabelLine), typeof(int), typeof(RangeBarMarker), new PropertyMetadata(0));

  /// <summary>Position on the bar's value scale where the tick is drawn. Off-scale markers are skipped.</summary>
  public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

  /// <summary>Brush for the tick line.</summary>
  public Brush Brush { get => (Brush)GetValue(BrushProperty); set => SetValue(BrushProperty, value); }

  /// <summary>Tick line width in device-independent pixels (default 2).</summary>
  public double Thickness { get => (double)GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }

  /// <summary>Extra px the tick extends above and below the bar band (default 0).</summary>
  public double Overhang { get => (double)GetValue(OverhangProperty); set => SetValue(OverhangProperty, value); }

  /// <summary>Optional caption drawn under the tick in the bar's label band (see <see cref="RangeBar.LabelHeight"/>).</summary>
  public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

  /// <summary>Brush for the label text; falls back to <see cref="Brush"/> when null.</summary>
  public Brush? LabelBrush { get => (Brush?)GetValue(LabelBrushProperty); set => SetValue(LabelBrushProperty, value); }

  /// <summary>Which stacked label row this caption uses (0 = first row). Use distinct lines for
  /// markers whose labels would otherwise overlap horizontally.</summary>
  public int LabelLine { get => (int)GetValue(LabelLineProperty); set => SetValue(LabelLineProperty, value); }

  protected override Freezable CreateInstanceCore() => new RangeBarMarker();
}

/// <summary>A change-notifying collection of <see cref="RangeBarMarker"/> for <see cref="RangeBar.Markers"/>.</summary>
public sealed class RangeBarMarkerCollection : FreezableCollection<RangeBarMarker> {
  protected override Freezable CreateInstanceCore() => new RangeBarMarkerCollection();
}
