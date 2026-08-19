using Crystal.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Crystal.Controls.Comet;

/// <summary>
/// Low-cost liveness overlay. The comet is a chain of small dots sampled along
/// the perimeter, each moved with a MatrixAnimationUsingPath (translation only).
/// Because a dot has no orientation it can never overshoot a corner, so the
/// trail hugs the path exactly - unlike a rigid dash/lozenge, which pokes out
/// tangentially on turns.
///
/// Per-frame cost is low: every dot is a tiny frozen ellipse driven by a
/// composited matrix translation - there is no per-frame stroke re-tessellation
/// like the dashed-path <see cref="LivenessIndicator"/>. Total elements =
/// TailSegments x DashCount. Shared properties and geometry live in
/// <see cref="LivenessIndicatorBase"/>.
/// </summary>
public class LivenessIndicatorLite : LivenessIndicatorBase {
  private readonly Canvas _root = new();

  public LivenessIndicatorLite() {
    Content = _root;
  }

  #region Dependency properties (Lite-only)

  public static readonly DependencyProperty PixelsPerSecondProperty = DependencyProperty.Register(
    nameof(PixelsPerSecond), typeof(double), typeof(LivenessIndicatorLite),
    new PropertyMetadata(0.0, OnLiteVisualChanged));
  /// <summary>Constant linear speed in px/s. When &gt; 0 this overrides LapSeconds
  /// (so differently sized controls travel at the same visual speed).</summary>
  public double PixelsPerSecond {
    get => (double)GetValue(PixelsPerSecondProperty);
    set => SetValue(PixelsPerSecondProperty, value);
  }

  public static readonly DependencyProperty SymmetricProperty = DependencyProperty.Register(
    nameof(Symmetric), typeof(bool), typeof(LivenessIndicatorLite),
    new PropertyMetadata(false, OnLiteVisualChanged));
  /// <summary>When true the comet is symmetric: a bright core in the middle that
  /// fades equally toward both ends (head == tail), instead of a bright head with
  /// a trailing tail. DashLength is the bright core; TailLength is the fade on each
  /// side, so the total span is DashLength + 2 * TailLength.</summary>
  public bool Symmetric {
    get => (bool)GetValue(SymmetricProperty);
    set => SetValue(SymmetricProperty, value);
  }

  private static void OnLiteVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    => ((LivenessIndicatorLite)d).Rebuild();

  #endregion

  protected override void Stop() {
    foreach (var child in _root.Children) {
      // Skip the frozen Transform.Identity that non-animated children (e.g. the
      // track path) carry by default - it is a MatrixTransform but cannot be
      // animated.
      if (child is UIElement el && el.RenderTransform is MatrixTransform { IsFrozen: false } mt)
        mt.BeginAnimation(MatrixTransform.MatrixProperty, null);
    }
  }

  protected override void Rebuild() {
    _root.Children.Clear();
    if (!IsLoaded) return;

    double w = ActualWidth, h = ActualHeight;
    double t = Math.Max(0.5, DashThickness);
    double inset = t / 2 + Inset;
    double innerW = w - 2 * inset, innerH = h - 2 * inset;
    if (innerW <= 0 || innerH <= 0) return;

    var (tl, tr, br, bl) = ClampRadii(ResolveCornerRadius(), innerW, innerH);
    bool clockwise = !Reverse;
    PathGeometry path = BuildRoundedRectPath(inset, inset, innerW, innerH, tl, tr, br, bl, clockwise);

    Color c = Color;

    if (ShowTrack) {
      _root.Children.Add(new Path {
        Data = path,
        Stroke = new SolidColorBrush(c) { Opacity = 0.14 },
        StrokeThickness = Math.Max(1, t * 0.6),
        StrokeStartLineCap = PenLineCap.Round,
        StrokeEndLineCap = PenLineCap.Round,
      });
    }

    if (!IsActive) return;

    int dashCount = Math.Max(1, DashCount);

    double perimeter = Perimeter(innerW, innerH, tl, tr, br, bl);
    if (perimeter <= 0) return;

    // Speed: explicit px/s wins; otherwise derive from LapSeconds.
    double lapSeconds = PixelsPerSecond > 0
      ? perimeter / PixelsPerSecond
      : Math.Max(0.1, LapSeconds);
    var dur = TimeSpan.FromSeconds(lapSeconds);

    // The comet is a chain of DOTS sampled along the path. A dot has no
    // orientation, so it can never stick out tangentially -> the trail hugs
    // the path perfectly around corners. Enough overlapping dots read as one
    // continuous bright dash. Each dot's brightness follows Brightness():
    //   - head/tail: bright leading core, fading tail behind it
    //   - symmetric: bright core in the middle, fading equally toward both ends
    bool symmetric = Symmetric;
    double core = Math.Max(0, DashLength);
    double fade = Math.Max(1, TailLength);
    double totalLen = symmetric ? core + 2 * fade : core + fade;
    int dots = Math.Max(2, TailSegments);

    // Draw dimmest dots first so the brightest end up on top.
    var order = Enumerable.Range(0, dots)
      .OrderBy(i => Brightness((double)i / (dots - 1) * totalLen, totalLen, core, fade, symmetric))
      .ToArray();
    int brightest = order[^1];

    for (int k = 0; k < dashCount; k++) {
      double phase = lapSeconds * k / dashCount; // even spacing of extra comets

      foreach (int i in order) {
        double d = (double)i / (dots - 1) * totalLen; // arc distance from the leading end
        double b = Brightness(d, totalLen, core, fade, symmetric);

        double diameter = Math.Max(0.6, t * (0.6 + 0.65 * b));
        double opacity = Math.Max(0.04, b);
        bool hot = i == brightest;

        Path dot;
        if (Glow && hot) {
          // Cheap composited glow: a radial-gradient halo (bright core fading to
          // transparent) instead of a DropShadowEffect blur. A blur forces an
          // extra render pass per region, which is costly across a dashboard full
          // of indicators; a gradient-filled ellipse is a plain composited draw.
          double glow = Math.Max(diameter, t * 4);
          var halo = new RadialGradientBrush {
            GradientStops = {
              new GradientStop(Lighten(c, 60), 0.0),
              new GradientStop(Color.FromArgb(0xC0, c.R, c.G, c.B), diameter / glow * 0.6),
              new GradientStop(Color.FromArgb(0x00, c.R, c.G, c.B), 1.0),
            },
          };
          halo.Freeze();
          dot = new Path {
            Data = new EllipseGeometry(new Point(0, 0), glow / 2, glow / 2),
            Fill = halo,
            Opacity = opacity,
          };
        } else {
          var brush = new SolidColorBrush(hot ? Lighten(c, 60) : c);
          brush.Freeze();
          dot = new Path {
            Data = new EllipseGeometry(new Point(0, 0), diameter / 2, diameter / 2),
            Fill = brush,
            Opacity = opacity,
          };
        }

        var mt = new MatrixTransform();
        dot.RenderTransform = mt;
        _root.Children.Add(dot);

        if (AnimationsEnabled) {
          // Each dot lags the leading end by the time to cover its arc distance d.
          // Shift the whole comet into the past (base) so no BeginTime > 0.
          double begin = -phase - (totalLen - d) / perimeter * lapSeconds;
          var anim = new MatrixAnimationUsingPath {
            PathGeometry = path,
            Duration = dur,
            RepeatBehavior = RepeatBehavior.Forever,
            DoesRotateWithTangent = false, // dots don't need orientation
            BeginTime = TimeSpan.FromSeconds(begin),
          };
          if (FrameRate > 0) Timeline.SetDesiredFrameRate(anim, FrameRate);
          mt.BeginAnimation(MatrixTransform.MatrixProperty, anim);
        } else {
          // Reduced motion: pin each dot at the position it would occupy at t=0
          // (leading end ahead, tail trailing behind it).
          double frac = ((double)k / dashCount + (totalLen - d) / perimeter) % 1.0;
          if (frac < 0) frac += 1.0;
          path.GetPointAtFractionLength(frac, out Point pt, out _);
          mt.Matrix = new Matrix(1, 0, 0, 1, pt.X, pt.Y);
        }
      }
    }
  }

  private static Color Lighten(Color c, int amount) => Color.FromArgb(
    0xFF,
    (byte)Math.Min(255, c.R + amount),
    (byte)Math.Min(255, c.G + amount),
    (byte)Math.Min(255, c.B + amount));

  // Brightness in [0,1] at arc distance d from the leading end of the comet.
  private static double Brightness(double d, double totalLen, double core, double fade, bool symmetric) {
    double edge = symmetric
      ? Math.Abs(d - totalLen / 2) - core / 2 // distance outside the centered core
      : d - core;                             // distance behind the leading core
    if (edge <= 0) return 1.0;                // inside the bright core
    return Math.Pow(Math.Clamp(1 - edge / fade, 0, 1), 1.3);
  }

  private static PathGeometry BuildRoundedRectPath(
      double x, double y, double w, double h,
      double tl, double tr, double br, double bl, bool clockwise) {
    var fig = new PathFigure { StartPoint = new Point(x + tl, y), IsClosed = true, IsFilled = false };
    SweepDirection sweep = clockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

    void Line(Point p) => fig.Segments.Add(new LineSegment(p, true));
    void Arc(Point p, double r) => fig.Segments.Add(new ArcSegment(p, new Size(r, r), 0, false, sweep, true));

    if (clockwise) {
      Line(new Point(x + w - tr, y));
      if (tr > 0) Arc(new Point(x + w, y + tr), tr);
      Line(new Point(x + w, y + h - br));
      if (br > 0) Arc(new Point(x + w - br, y + h), br);
      Line(new Point(x + bl, y + h));
      if (bl > 0) Arc(new Point(x, y + h - bl), bl);
      Line(new Point(x, y + tl));
      if (tl > 0) Arc(new Point(x + tl, y), tl);
    } else {
      if (tl > 0) Arc(new Point(x, y + tl), tl); else Line(new Point(x, y + tl));
      Line(new Point(x, y + h - bl));
      if (bl > 0) Arc(new Point(x + bl, y + h), bl);
      Line(new Point(x + w - br, y + h));
      if (br > 0) Arc(new Point(x + w, y + h - br), br);
      Line(new Point(x + w, y + tr));
      if (tr > 0) Arc(new Point(x + w - tr, y), tr);
      Line(new Point(x + tl, y));
    }

    var pg = new PathGeometry();
    pg.Figures.Add(fig);
    pg.Freeze();
    return pg;
  }
}
