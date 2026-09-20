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
[ValueConversion(typeof(double), typeof(Brush))]
public sealed class ValueToBrushConverter : IValueConverter {
  /// <summary>
  /// Lower edge of the first band. Values below this return <see cref="Fallback"/>.
  /// </summary>
  public double Minimum { get; set; }

  /// <summary>
  /// Width of each band (the "gate" between colours). Must be &gt; 0 or the converter falls back.
  /// </summary>
  public double Step { get; set; } = 20;

  /// <summary>
  /// Band brushes, low to high. Index i covers [Minimum + i*Step, Minimum + (i+1)*Step).
  /// </summary>
  public Collection<Brush> Brushes { get; } = [];

  /// <summary>
  /// Returned when the value falls outside every band (below Minimum or past the top edge).
  /// </summary>
  public Brush Fallback { get; set; } = System.Windows.Media.Brushes.Gray;

  /// <summary>
  /// Converts a numeric value to a <see cref="Brush"/> based on which band it falls into. 
  /// If the value is below <see cref="Minimum"/> or above the last band's upper edge, 
  /// returns <see cref="Fallback"/>. If the value is not a valid number, also returns <see cref="Fallback"/>.
  /// </summary>
  /// <param name="value">The numeric value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted value.</returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (Brushes.Count == 0 || Step <= 0) return Fallback;
    if (!TryToDouble(value, culture, out double v)) return Fallback;

    double top = Minimum + Step * Brushes.Count;
    if (v < Minimum || v > top) return Fallback;

    int index = (int)Math.Floor((v - Minimum) / Step);
    if (index >= Brushes.Count) index = Brushes.Count - 1; // value sitting exactly on the top edge
    return Brushes[index] ?? Fallback;
  }

  /// <summary>
  /// Not supported. This converter does not support converting back from a <see cref="Brush"/> to a numeric value.
  /// </summary>
  /// <param name="value">The value to convert back.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted values.</returns>
  /// <exception cref="NotSupportedException"></exception>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();

  /// <summary>
  /// Attempts to convert an object to a double, using the specified culture for conversion.
  /// </summary>
  /// <param name="value">The object to convert.</param>
  /// <param name="culture">The culture to use for conversion.</param>
  /// <param name="result">When this method returns, contains the converted value if the conversion was successful; otherwise, zero.</param>
  /// <returns>true if the conversion was successful; otherwise, false.</returns>
  private static bool TryToDouble(object value, CultureInfo culture, out double result) {
    switch (value) {
      case double d:
        result = d;
        return true;
      case IConvertible c:
        try {
          result = c.ToDouble(culture);
          return true;
        }
        catch (FormatException) {
        }
        catch (InvalidCastException) {
        }
        break;
    }
    result = 0;
    return false;
  }
}
