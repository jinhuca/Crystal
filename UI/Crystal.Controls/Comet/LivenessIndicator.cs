using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Crystal.Controls.Comet;

/// <summary>
/// A "liveness" overlay: one or two bright dashes with a fading tail (comet)
/// that slowly travel around the perimeter of a rounded rectangle. Place it in
/// the same cell as the control you want to decorate and match its size /
/// CornerRadius.
///
/// The comet is built from layered dashed strokes whose StrokeDashOffset is
/// animated. This gives perfect corners but re-tessellates the stroke every
/// frame (CPU) - see <see cref="LivenessIndicatorLite"/> for a cheaper dot-based
/// variant. Shared properties and geometry live in <see cref="LivenessIndicatorBase"/>.
/// </summary>
public class LivenessIndicator : LivenessIndicatorBase {
  /// <summary>
  /// The root grid that holds the animated paths. This grid is used to layer multiple paths for the comet effect.
  /// </summary>
  private readonly Grid _root = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="LivenessIndicator"/> class. 
  /// This constructor sets up the root grid and assigns it as the content of the control.
  /// </summary>
  public LivenessIndicator() {
    Content = _root;
  }

  /// <summary>
  /// Stops the animations for all child shapes in the root grid. This method is called when the control 
  /// is unloaded or when the animations need to be stopped.
  /// </summary>
  protected override void Stop() {
    foreach (var child in _root.Children) {
      if (child is Shape s) {
        s.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
      }
    }
  }

  /// <summary>
  /// Rebuilds the comet paths based on the current properties of the control. This method clears the 
  /// existing children in the root grid and creates new paths for the comet effect, taking into account 
  /// the current size, corner radius, dash count, and other properties. The paths are animated to create 
  /// the appearance of a moving comet around the perimeter of a rounded rectangle.
  /// </summary>
  protected override void Rebuild() {
    _root.Children.Clear();
    if (!IsLoaded) {
      return;
    }

    double w = ActualWidth, h = ActualHeight;
    double t = Math.Max(0.5, DashThickness);
    double inset = t / 2 + Inset;
    double innerW = w - 2 * inset, innerH = h - 2 * inset;
    if (innerW <= 0 || innerH <= 0) {
      return;
    }

    var (tl, tr, br, bl) = ClampRadii(ResolveCornerRadius(), innerW, innerH);
    Geometry geo = BuildRoundedRect(inset, inset, innerW, innerH, tl, tr, br, bl);
    double perimeter = Perimeter(innerW, innerH, tl, tr, br, bl);
    if (perimeter <= 0) {
      return;
    }

    int dashCount = Math.Max(1, DashCount);
    double patternLen = perimeter / dashCount;
    if (DashLength >= patternLen) {
      return; // dash bigger than its slot; nothing sensible to draw
    }

    Color c = Color;

    if (ShowTrack) {
      _root.Children.Add(new Path {
        Data = geo,
        Stroke = new SolidColorBrush(c) { Opacity = 0.14 },
        StrokeThickness = Math.Max(1, t * 0.6),
        StrokeStartLineCap = PenLineCap.Round,
        StrokeEndLineCap = PenLineCap.Round,
      });
    }

    if (!IsActive) {
      return;
    }

    int n = Math.Max(0, TailSegments);
    double stepArc = n > 0 ? TailLength / n : 0;
    var duration = new Duration(TimeSpan.FromSeconds(Math.Max(0.1, LapSeconds) / dashCount));

    // Draw tail-end first, head last, so the head sits on top.
    for (int comet = 0; comet < dashCount; comet++) {
      double cometArc = comet * patternLen; // offset for the second comet
      for (int i = n; i >= 0; i--) {
        double frac = n > 0 ? (double)i / n : 0;
        double thickness = i == 0 ? t * 1.25 : t;
        double dashU = DashLength / thickness;
        double patU = patternLen / thickness;
        double gapU = Math.Max(0.01, patU - dashU);

        var path = new Path {
          Data = geo,
          Stroke = new SolidColorBrush(c),
          StrokeThickness = thickness,
          StrokeDashArray = new DoubleCollection { dashU, gapU },
          StrokeStartLineCap = PenLineCap.Round,
          StrokeEndLineCap = PenLineCap.Round,
          StrokeDashCap = PenLineCap.Round,
          Opacity = Math.Pow(1 - frac, 1.5),
        };

        if (Glow && i == 0) {
          path.Effect = new DropShadowEffect {
            Color = c,
            ShadowDepth = 0,
            BlurRadius = Math.Max(6, t * 4),
            Opacity = 0.9,
          };
        }

        // Trailing copies sit "behind" the head (opposite the travel direction).
        double dir = Reverse ? -1 : 1;
        double baseArc = cometArc - dir * i * stepArc;
        double from = baseArc / thickness;
        double to = from - dir * patU; // one full pattern -> seamless loop

        if (AnimationsEnabled) {
          var anim = new DoubleAnimation {
            From = from,
            To = to,
            Duration = duration,
            RepeatBehavior = RepeatBehavior.Forever,
          };
          if (FrameRate > 0) {
            Timeline.SetDesiredFrameRate(anim, FrameRate);
          }

          path.BeginAnimation(Shape.StrokeDashOffsetProperty, anim);
        }
        else {
          path.StrokeDashOffset = from; // reduced motion: static comet at rest
        }
        _root.Children.Add(path);
      }
    }
  }

  /// <summary>
  /// Builds a rounded rectangle geometry with specified corner radii. This method creates a StreamGeometry 
  /// that represents a rectangle with rounded corners, where each corner can have a different radius. The 
  /// geometry is constructed by defining the path of the rectangle and adding arcs for the rounded corners.
  /// </summary>
  /// <param name="x">x</param>
  /// <param name="y">y</param>
  /// <param name="w">w</param>
  /// <param name="h">h</param>
  /// <param name="tl">tl</param>
  /// <param name="tr">tr</param>
  /// <param name="br">br</param>
  /// <param name="bl">bl</param>
  /// <returns></returns>
  private static Geometry BuildRoundedRect(
    double x, double y, double w, double h, double tl, double tr, double br, double bl) {
    var g = new StreamGeometry();
    using (var ctx = g.Open()) {
      ctx.BeginFigure(new Point(x + tl, y), isFilled: false, isClosed: true);
      ctx.LineTo(new Point(x + w - tr, y), true, true);
      if (tr > 0) {
        ctx.ArcTo(new Point(x + w, y + tr), new Size(tr, tr), 0, false, SweepDirection.Clockwise, true, true);
      }

      ctx.LineTo(new Point(x + w, y + h - br), true, true);
      if (br > 0) {
        ctx.ArcTo(new Point(x + w - br, y + h), new Size(br, br), 0, false, SweepDirection.Clockwise, true, true);
      }

      ctx.LineTo(new Point(x + bl, y + h), true, true);
      if (bl > 0) {
        ctx.ArcTo(new Point(x, y + h - bl), new Size(bl, bl), 0, false, SweepDirection.Clockwise, true, true);
      }

      ctx.LineTo(new Point(x, y + tl), true, true);
      if (tl > 0) {
        ctx.ArcTo(new Point(x + tl, y), new Size(tl, tl), 0, false, SweepDirection.Clockwise, true, true);
      }
    }
    g.Freeze();
    return g;
  }
}
