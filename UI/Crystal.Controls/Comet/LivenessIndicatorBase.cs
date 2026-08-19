using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Controls.Comet;
/// <summary>
/// Shared plumbing for the liveness overlays: the common dependency properties,
/// the visible/loaded lifecycle wiring, and the rounded-rect geometry helpers.
/// Subclasses supply the actual rendering in <see cref="Rebuild"/> and tear it
/// down in <see cref="Stop"/>.
/// </summary>
public abstract class LivenessIndicatorBase : UserControl {
  protected LivenessIndicatorBase() {
    IsHitTestVisible = false; // purely decorative
    Loaded += (_, _) => Rebuild();
    Unloaded += (_, _) => Stop();
    SizeChanged += (_, _) => Rebuild();
    // Free the animation clocks whenever the control isn't actually shown
    // (collapsed panel, hidden tab, scrolled out of view).
    IsVisibleChanged += (_, e) => { if ((bool)e.NewValue) Rebuild(); else Stop(); };
  }

  /// <summary>Rebuild the visual tree and (re)start the animation.</summary>
  protected abstract void Rebuild();

  /// <summary>Detach the running animation clocks.</summary>
  protected abstract void Stop();

  #region Dependency properties

  public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
    nameof(Color), typeof(Color), typeof(LivenessIndicatorBase),
    new PropertyMetadata(Color.FromRgb(0x4D, 0xD8, 0xFF), OnVisualChanged));
  /// <summary>Base color of the comet.</summary>
  public Color Color {
    get => (Color)GetValue(ColorProperty);
    set => SetValue(ColorProperty, value);
  }

  public static readonly DependencyProperty DashThicknessProperty = DependencyProperty.Register(
    nameof(DashThickness), typeof(double), typeof(LivenessIndicatorBase),
    new PropertyMetadata(3.0, OnVisualChanged));
  /// <summary>Stroke width / dot size of the traveling dash.</summary>
  public double DashThickness {
    get => (double)GetValue(DashThicknessProperty);
    set => SetValue(DashThicknessProperty, value);
  }

  public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
    nameof(CornerRadius), typeof(CornerRadius), typeof(LivenessIndicatorBase),
    new PropertyMetadata(new CornerRadius(8), OnVisualChanged));
  /// <summary>Corner radii of the traced path. If left unset, a nearby Border's
  /// CornerRadius is adopted automatically (see <see cref="ResolveCornerRadius"/>).</summary>
  public CornerRadius CornerRadius {
    get => (CornerRadius)GetValue(CornerRadiusProperty);
    set => SetValue(CornerRadiusProperty, value);
  }

  public static readonly DependencyProperty DashLengthProperty = DependencyProperty.Register(
    nameof(DashLength), typeof(double), typeof(LivenessIndicatorBase),
    new PropertyMetadata(22.0, OnVisualChanged));
  /// <summary>Length (px) of the bright head / core.</summary>
  public double DashLength {
    get => (double)GetValue(DashLengthProperty);
    set => SetValue(DashLengthProperty, value);
  }

  public static readonly DependencyProperty TailLengthProperty = DependencyProperty.Register(
    nameof(TailLength), typeof(double), typeof(LivenessIndicatorBase),
    new PropertyMetadata(80.0, OnVisualChanged));
  /// <summary>Length (px) of the fading tail behind the head.</summary>
  public double TailLength {
    get => (double)GetValue(TailLengthProperty);
    set => SetValue(TailLengthProperty, value);
  }

  public static readonly DependencyProperty TailSegmentsProperty = DependencyProperty.Register(
    nameof(TailSegments), typeof(int), typeof(LivenessIndicatorBase),
    new PropertyMetadata(16, OnVisualChanged));
  /// <summary>Resolution of the tail. Interpreted per subclass (layered stroke
  /// copies for the dashed variant, sampled dots for the Lite variant).</summary>
  public int TailSegments {
    get => (int)GetValue(TailSegmentsProperty);
    set => SetValue(TailSegmentsProperty, value);
  }

  public static readonly DependencyProperty LapSecondsProperty = DependencyProperty.Register(
    nameof(LapSeconds), typeof(double), typeof(LivenessIndicatorBase),
    new PropertyMetadata(4.0, OnVisualChanged));
  /// <summary>Seconds for the head to travel once around the perimeter.</summary>
  public double LapSeconds {
    get => (double)GetValue(LapSecondsProperty);
    set => SetValue(LapSecondsProperty, value);
  }

  public static readonly DependencyProperty DashCountProperty = DependencyProperty.Register(
    nameof(DashCount), typeof(int), typeof(LivenessIndicatorBase),
    new PropertyMetadata(1, OnVisualChanged));
  /// <summary>Number of equally spaced comets.</summary>
  public int DashCount {
    get => (int)GetValue(DashCountProperty);
    set => SetValue(DashCountProperty, value);
  }

  public static readonly DependencyProperty InsetProperty = DependencyProperty.Register(
    nameof(Inset), typeof(double), typeof(LivenessIndicatorBase),
    new PropertyMetadata(0.0, OnVisualChanged));
  /// <summary>Extra inward offset of the traced path from the bounds (beyond half the stroke).</summary>
  public double Inset {
    get => (double)GetValue(InsetProperty);
    set => SetValue(InsetProperty, value);
  }

  public static readonly DependencyProperty ReverseProperty = DependencyProperty.Register(
    nameof(Reverse), typeof(bool), typeof(LivenessIndicatorBase),
    new PropertyMetadata(false, OnVisualChanged));
  /// <summary>Travel counter-clockwise instead of clockwise.</summary>
  public bool Reverse {
    get => (bool)GetValue(ReverseProperty);
    set => SetValue(ReverseProperty, value);
  }

  public static readonly DependencyProperty GlowProperty = DependencyProperty.Register(
    nameof(Glow), typeof(bool), typeof(LivenessIndicatorBase),
    new PropertyMetadata(true, OnVisualChanged));
  /// <summary>Add a soft glow halo around the head.</summary>
  public bool Glow {
    get => (bool)GetValue(GlowProperty);
    set => SetValue(GlowProperty, value);
  }

  public static readonly DependencyProperty ShowTrackProperty = DependencyProperty.Register(
    nameof(ShowTrack), typeof(bool), typeof(LivenessIndicatorBase),
    new PropertyMetadata(true, OnVisualChanged));
  /// <summary>Draw a faint full outline so the travel path is always visible.</summary>
  public bool ShowTrack {
    get => (bool)GetValue(ShowTrackProperty);
    set => SetValue(ShowTrackProperty, value);
  }

  public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
    nameof(IsActive), typeof(bool), typeof(LivenessIndicatorBase),
    new PropertyMetadata(true, OnVisualChanged));
  /// <summary>Start/stop the animation.</summary>
  public bool IsActive {
    get => (bool)GetValue(IsActiveProperty);
    set => SetValue(IsActiveProperty, value);
  }

  public static readonly DependencyProperty FrameRateProperty = DependencyProperty.Register(
    nameof(FrameRate), typeof(int), typeof(LivenessIndicatorBase),
    new PropertyMetadata(0, OnVisualChanged));
  /// <summary>Cap the animation timeline frame rate (fps). 0 = uncapped (default 60).
  /// A slow indicator looks fine at 30 or even 20 and roughly halves per-frame cost -
  /// useful when many indicators run at once.</summary>
  public int FrameRate {
    get => (int)GetValue(FrameRateProperty);
    set => SetValue(FrameRateProperty, value);
  }

  private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    => ((LivenessIndicatorBase)d).Rebuild();

  #endregion

  // In the common "Grid -> Border + overlay" (or Border -> indicator) layout the
  // indicator's corners just repeat the Border's. So if the caller never set
  // CornerRadius explicitly, adopt a nearby Border's CornerRadius instead of the
  // default - one place to change the shape.
  protected CornerRadius ResolveCornerRadius() {
    if (ReadLocalValue(CornerRadiusProperty) != DependencyProperty.UnsetValue)
      return CornerRadius; // caller set it explicitly - honor it
    return FindAmbientBorder()?.CornerRadius ?? CornerRadius;
  }

  private Border? FindAmbientBorder() {
    var parent = VisualTreeHelper.GetParent(this);
    if (parent is Border nested) return nested;            // indicator inside the Border
    if (parent is Panel panel)                             // indicator overlaid beside the Border
      foreach (var child in panel.Children)
        if (child is Border sibling) return sibling;
    return null;
  }

  // False when the OS "show animations" / reduced-motion setting is off (also in
  // some remote/battery-saver sessions). When false, subclasses render a static
  // marker instead of starting animation clocks - respects the user's preference
  // and costs nothing per frame.
  protected static bool AnimationsEnabled => SystemParameters.ClientAreaAnimation;

  protected static (double tl, double tr, double br, double bl) ClampRadii(
      CornerRadius r, double w, double h) {
    double max = Math.Min(w, h) / 2;
    double c(double v) => Math.Max(0, Math.Min(v, max));
    return (c(r.TopLeft), c(r.TopRight), c(r.BottomRight), c(r.BottomLeft));
  }

  protected static double Perimeter(double w, double h, double tl, double tr, double br, double bl) {
    double straight = (w - tl - tr) + (h - tr - br) + (w - br - bl) + (h - bl - tl);
    double arcs = (Math.PI / 2) * (tl + tr + br + bl); // four quarter-circles
    return straight + arcs;
  }
}
