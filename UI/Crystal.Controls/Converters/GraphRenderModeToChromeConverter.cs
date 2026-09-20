using System.Globalization;
using System.Windows.Data;
using Crystal.Controls.PerformanceGraphs;

namespace Crystal.Controls.Converters;

/// <summary>
/// Maps the global <see cref="GraphRenderMode"/> (the shell's title-bar Line/Dot toggle) to whether a
/// dashboard graph should show its chrome (grid + axis border). Dot mode reads as a bare scatter, so
/// the grid and border are hidden; Line mode keeps them. A tile binds its
/// <see cref="PerformanceGraph.Grid"/> / <see cref="PerformanceGraph.Border"/> through this converter
/// to <see cref="GraphAppearance.Current"/>'s <see cref="GraphAppearance.Mode"/>, so flipping the
/// toggle shows or hides the chrome in place.
/// </summary>
[ValueConversion(typeof(GraphRenderMode), typeof(bool))]
public sealed class GraphRenderModeToChromeConverter : IValueConverter {
  /// <summary>
  /// Converts a <see cref="GraphRenderMode"/> value to a <see cref="bool"/> indicating whether the chrome should be shown.
  /// </summary>
  /// <param name="value">The GraphRenderMode value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted bool value.</returns>
  /// <returns></returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is not GraphRenderMode.Dot;

  /// <summary>
  /// Not supported. This converter does not support converting back from a bool to a GraphRenderMode.
  /// </summary>
  /// <param name="value">The bool value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns></returns>
  /// <returns></returns>
  /// <exception cref="NotSupportedException"></exception>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    throw new NotSupportedException();
}
