using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Crystal.Controls.Converters;

/// <summary>
/// Maps a numeric value to a <see cref="Brush"/> by which fixed-width band it falls into, for
/// binding a bar's fill colour to its own <c>Value</c> (e.g. a load meter that shifts green→red as
/// it fills). Bands are <see cref="Step"/> wide starting at <see cref="Minimum"/>: the first brush
/// in <see cref="Brushes"/> covers [Minimum, Minimum+Step), the next the following Step, and so on.
/// A value sitting exactly on the top edge maps to the last brush; anything below <see cref="Minimum"/>
/// or past the last band's upper edge returns <see cref="Fallback"/>.
/// <para>
/// Declared in XAML with the band brushes as direct content (low to high), e.g. Step="20" over
/// Minimum="0" with five brushes gives the 0-20 / 20-40 / … / 80-100 gates, grey outside.
/// </para>
/// </summary>
[ContentProperty(nameof(Brushes))]
public sealed class ValueToBrushConverter : IValueConverter {
  /// <summary>Lower edge of the first band. Values below this return <see cref="Fallback"/>.</summary>
  public double Minimum { get; set; }

  /// <summary>Width of each band (the "gate" between colours). Must be &gt; 0 or the converter falls back.</summary>
  public double Step { get; set; } = 20;

  /// <summary>Band brushes, low to high. Index i covers [Minimum + i*Step, Minimum + (i+1)*Step).</summary>
  public Collection<Brush> Brushes { get; } = [];

  /// <summary>Returned when the value falls outside every band (below Minimum or past the top edge).</summary>
  public Brush Fallback { get; set; } = System.Windows.Media.Brushes.Gray;

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (Brushes.Count == 0 || Step <= 0) return Fallback;
    if (!TryToDouble(value, culture, out double v)) return Fallback;

    double top = Minimum + Step * Brushes.Count;
    if (v < Minimum || v > top) return Fallback;

    int index = (int)Math.Floor((v - Minimum) / Step);
    if (index >= Brushes.Count) index = Brushes.Count - 1; // value sitting exactly on the top edge
    return Brushes[index] ?? Fallback;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();

  private static bool TryToDouble(object value, CultureInfo culture, out double result) {
    switch (value) {
      case double d:
        result = d;
        return true;
      case IConvertible c:
        try {
          result = c.ToDouble(culture);
          return true;
        } catch (FormatException) {
        } catch (InvalidCastException) {
        }
        break;
    }
    result = 0;
    return false;
  }
}
