using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Kinds;

// For the discrete renderers (Bar, SegmentedBar) each sample is a separate rectangle, so fill and
// outline are drawn as one uniform solid: the fill collapses to a solid colour (a gradient, meant
// as one continuous glow, would otherwise restart inside every rectangle and even its brightest
// stop is only partly opaque) and the stroke pen is that SAME colour, so a bar/segment reads as a
// single solid block instead of a two-tone fill + accent outline. A gradient is flattened to its
// most-opaque stop at full alpha; an already-solid brush is used as-is. The resolved brush + pen are
// cached and rebuilt only when the source fill or the stroke thickness changes, so a static fill
// isn't reallocated every frame.
internal sealed class SolidFillCache {
  private Brush? _source;
  private double _thickness = double.NaN;
  private Brush? _fill;
  private Pen? _pen;

  // Returns the solid fill brush and a matching-colour stroke pen for the given fill and outline
  // thickness. Both share one brush, so the bar/segment fill and border are identical.
  public (Brush fill, Pen pen) Resolve(Brush fill, double strokeThickness) {
    if (ReferenceEquals(fill, _source) && strokeThickness == _thickness) return (_fill!, _pen!);

    Brush solid = ToSolid(fill);
    var pen = new Pen(solid, strokeThickness);
    pen.Freeze();

    _source = fill;
    _thickness = strokeThickness;
    _fill = solid;
    _pen = pen;
    return (solid, pen);
  }

  private static Brush ToSolid(Brush fill) {
    if (fill is not GradientBrush gradient || gradient.GradientStops.Count == 0) return fill;

    GradientStop pick = gradient.GradientStops[0];
    foreach (GradientStop stop in gradient.GradientStops)
      if (stop.Color.A > pick.Color.A) pick = stop;

    Color c = pick.Color;
    var solid = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B));
    solid.Freeze();
    return solid;
  }
}
