using System;
using System.Globalization;
using System.Windows.Data;
using Crystal.Controls.PerformanceGraphs;

namespace Crystal.Controls.Converters;

/// <summary>
/// Computes the Height a <see cref="PerformanceGraphLite"/> needs so its dots render perfectly
/// square, given its own ActualWidth/Capacity/Rows - via
/// <see cref="PerformanceGraphLite.SquareDotAspectRatio"/>, the exact same math the control's own
/// rendering uses, not a separately-maintained approximation.
/// </summary>
/// <remarks>
/// Bind all three inputs from the SAME element via a MultiBinding, e.g.:
/// <code>
/// &lt;graphs:PerformanceGraphLite x:Name="Lite1" Capacity="30" HorizontalAlignment="Stretch"&gt;
///   &lt;graphs:PerformanceGraphLite.Height&gt;
///     &lt;MultiBinding Converter="{StaticResource CapToRatioConverter}"&gt;
///       &lt;Binding Path="ActualWidth" RelativeSource="{RelativeSource Self}"/&gt;
///       &lt;Binding Path="Capacity" RelativeSource="{RelativeSource Self}"/&gt;
///       &lt;Binding Path="Rows" RelativeSource="{RelativeSource Self}"/&gt;
///     &lt;/MultiBinding&gt;
///   &lt;/graphs:PerformanceGraphLite.Height&gt;
/// &lt;/graphs:PerformanceGraphLite&gt;
/// </code>
/// No ConverterParameter is used or needed - Capacity and Rows come from the graph's own real,
/// live DP values (both are now full dependency properties), so the ratio can never drift out of
/// sync with what the graph is actually configured with the way a hand-typed
/// <c>ConverterParameter="30,1.0"</c> string could (and, before this rewrite, silently always did:
/// the previous single-value version read <c>parameter</c> via a bare <c>ToString()</c> call -
/// with no receiver, that calls the CONVERTER's own <c>ToString()</c>, not the parameter's, so it
/// could never parse as a number and the capacity argument was silently ignored every time).
/// </remarks>
public class CapacityToRatioConverter : IMultiValueConverter {
  public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
    // Safe placeholder while WPF is still resolving initial layout sizes, or if this converter
    // gets bound to something other than the 3-value (Width, Capacity, Rows) shape it expects.
    if (values.Length < 3
        || values[0] is not double width || double.IsNaN(width) || width <= 0
        || values[1] is not int capacity
        || values[2] is not int rows) {
      return 20.0;
    }

    double ratio = PerformanceGraphLite.SquareDotAspectRatio(rows, capacity);
    return Math.Max(20.0, width * ratio);
  }

  public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
