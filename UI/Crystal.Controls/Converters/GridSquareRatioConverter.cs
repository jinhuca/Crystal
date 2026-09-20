using System.Globalization;
using System.Windows.Data;
using Crystal.Controls.PerformanceGraphs;

namespace Crystal.Controls.Converters;

/// <summary>
/// Computes the Height a <see cref="PerformanceGraph"/> needs so its grid cells render as perfect
/// squares, given its own ActualWidth/GridRows/GridColumns - via
/// <see cref="PerformanceGraph.SquareGridAspectRatio"/>, the exact same
/// <c>cellWidth = Width / GridColumns</c>, <c>cellHeight = Height / GridRows</c> math
/// <c>GridRenderer</c> itself uses internally, not a separately-maintained approximation.
/// </summary>
/// <remarks>
/// Bind all three inputs from the SAME element via a MultiBinding, e.g.:
/// <code>
/// &lt;graphs:PerformanceGraph x:Name="Graph1" HorizontalAlignment="Stretch"&gt;
///   &lt;graphs:PerformanceGraph.Height&gt;
///     &lt;MultiBinding Converter="{StaticResource GridSquareRatioConverter}"&gt;
///       &lt;Binding Path="ActualWidth" RelativeSource="{RelativeSource Self}"/&gt;
///       &lt;Binding Path="GridRows" RelativeSource="{RelativeSource Self}"/&gt;
///       &lt;Binding Path="GridColumns" RelativeSource="{RelativeSource Self}"/&gt;
///     &lt;/MultiBinding&gt;
///   &lt;/graphs:PerformanceGraph.Height&gt;
/// &lt;/graphs:PerformanceGraph&gt;
/// </code>
/// This converter only knows about the GRID's own row/column counts - it has no opinion on
/// whether <see cref="PerformanceGraph.GridColumns"/> happens to equal
/// <see cref="PerformanceGraph.Capacity"/> or not. Tie those together separately (e.g. a Style
/// Setter binding <c>GridColumns</c> to <c>Capacity</c>) if that's the policy you want; this
/// converter will square whatever <c>GridRows</c>/<c>GridColumns</c> actually are at the time.
/// </remarks>
[ValueConversion(typeof(object[]), typeof(double))]
public class GridSquareRatioConverter : IMultiValueConverter {
  /// <summary>
  /// Computes the Height a <see cref="PerformanceGraph"/> needs so its grid cells render as perfect squares, 
  /// given its own ActualWidth/GridRows/GridColumns - via <see cref="PerformanceGraph.SquareGridAspectRatio"/>, 
  /// the exact same <c>cellWidth = Width / GridColumns</c>, <c>cellHeight = Height / GridRows</c> math <c>GridRenderer</c> 
  /// itself uses internally, not a separately-maintained approximation.
  /// </summary>
  /// <param name="values">The values to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted value.</returns>
  /// <returns></returns>
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    // Safe placeholder while WPF is still resolving initial layout sizes, or if this converter
    // gets bound to something other than the 3-value (Width, GridRows, GridColumns) shape it expects.
    if (values.Length < 3
        || values[0] is not double width || double.IsNaN(width) || width <= 0
        || values[1] is not int gridRows
        || values[2] is not int gridColumns) {
      return 20.0;
    }

    double ratio = PerformanceGraph.SquareGridAspectRatio(gridRows, gridColumns);
    return Math.Max(20.0, width * ratio);
  }

  /// <summary>
  /// Not supported. This converter does not support converting back from a formatted string to the original values.
  /// </summary>
  /// <param name="value">The value to convert back.</param>
  /// <param name="targetTypes">The types of the target values.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted values.</returns>
  /// <exception cref="NotSupportedException"></exception>
  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}
