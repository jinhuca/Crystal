using Crystal.Controls.PerformanceGraphs.Styles;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Controls.PerformanceGraphs.Renders;

/// <summary>
/// Draws horizontal reference lines at fixed data values — used for session extremes (the lowest/
/// highest sample seen) so a past dip or spike stays visible after the live line has recovered. When
/// a label is supplied, the value is also written in the corner the line hugs, so a marker reads as a
/// number rather than just a position.
/// </summary>
internal sealed class MarkerRenderer {
  /// <summary>
  /// The font size for the marker labels. This is a constant value that determines the size of the 
  /// text used for the labels.
  /// </summary>
  private const double LabelFontSize = 9;

  /// <summary>
  /// The padding around the marker labels. This is a constant value that determines the space between 
  /// the label text and the marker line.
  /// </summary>
  private const double LabelPadding = 2;

  /// <summary>
  /// The typeface used for the marker labels. This is a constant value that specifies the font family
  /// </summary>
  private static readonly Typeface LabelTypeface = new("Consolas");

  /// <summary>
  /// A marker's label only changes when its extreme does (rare), but OnRender re-runs every sample,
  /// and FormattedText is comparatively expensive to build. Cache the last one so a steady extreme
  /// reuses it frame after frame instead of re-shaping the glyphs each time. Keyed on everything the
  /// text depends on; the low and high markers each own an instance, so their labels don't thrash a
  /// shared single-entry cache.
  /// </summary>
  private string? _cachedLabel;

  /// <summary>
  /// The brush used for the marker labels. This is cached to avoid unnecessary allocations and to ensure 
  /// that the same brush is used for rendering the text.
  /// </summary>
  private Brush? _cachedBrush;

  /// <summary>
  /// The DPI (dots per inch) value used for rendering the marker labels. This is cached to avoid unnecessary 
  /// allocations and to ensure that the same DPI value is used for rendering the text.
  /// </summary>
  private double _cachedDpi = double.NaN;

  /// <summary>
  /// The cached FormattedText object used for rendering the marker labels. This is cached to avoid unnecessary 
  /// allocations and to ensure that the same FormattedText object is used for rendering the text.
  /// </summary>
  private FormattedText? _cachedText;

  /// <summary>
  /// Indicates whether the marker label should be biased towards the top of the plot.
  /// true for the high marker (label sits below the line, growing down into the plot),
  /// false for the low marker (label sits above the line, growing up) — so neither label is clipped
  /// at the plot edge and the two never overlap when the markers are far apart.
  /// </summary>
  public void Draw(DrawingContext dc, Rect bounds, GraphStyle style, double value, double minValue, double maxValue,
      string? label = null, bool topBiased = false, double pixelsPerDip = 1.0) {
    if (style.MarkerPen is null) return;
    if (double.IsNaN(value)) return;
    if (bounds.Width <= 0 || bounds.Height <= 0) return;

    double range = maxValue - minValue;
    if (range <= 0) return;

    double t = (value - minValue) / range;
    t = t < 0 ? 0 : (t > 1 ? 1 : t);
    double y = bounds.Bottom - t * bounds.Height;
    dc.DrawLine(style.MarkerPen, new Point(bounds.Left, y), new Point(bounds.Right, y));

    if (string.IsNullOrEmpty(label)) return;
    FormattedText text = GetOrBuildText(label, style.MarkerPen.Brush, pixelsPerDip);
    // Right-align against the plot edge; drop below the line for the high marker, lift above it for
    // the low one, then clamp so a marker near an edge keeps its whole label inside the plot.
    double x = bounds.Right - text.Width - LabelPadding;
    double textY = topBiased ? y + LabelPadding : y - text.Height - LabelPadding;
    if (textY < bounds.Top) textY = bounds.Top;
    if (textY + text.Height > bounds.Bottom) textY = bounds.Bottom - text.Height;
    dc.DrawText(text, new Point(x, textY));
  }

  /// <summary>
  /// Gets or builds a FormattedText object for the specified label, brush, and pixelsPerDip. 
  /// If the cached values match the provided parameters, the cached FormattedText is returned.
  /// </summary>
  /// <param name="label">The label text to render.</param>
  /// <param name="brush">The brush used for rendering the text.</param>
  /// <param name="pixelsPerDip">The pixels per DIP ratio.</param>
  /// <returns>The FormattedText object.</returns>
  private FormattedText GetOrBuildText(string label, Brush brush, double pixelsPerDip) {
    if (_cachedText != null && _cachedLabel == label &&
        ReferenceEquals(_cachedBrush, brush) && _cachedDpi == pixelsPerDip) {
      return _cachedText;
    }

    var text = new FormattedText(label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
        LabelTypeface, LabelFontSize, brush, pixelsPerDip);
    _cachedLabel = label;
    _cachedBrush = brush;
    _cachedDpi = pixelsPerDip;
    _cachedText = text;
    return text;
  }
}
