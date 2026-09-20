using System.Globalization;
using System.Windows.Data;
using Crystal.Controls.PerformanceGraphs;

namespace Crystal.Controls.Converters;

/// <summary>
/// Maps the global <see cref="GraphRenderMode"/> (written by the shell's title-bar Line/Dot toggle)
/// to a <see cref="PerformanceGraph"/>'s <see cref="DisplayMode"/>. A dashboard graph binds its
/// <see cref="PerformanceGraph.DisplayMode"/> through this converter to
/// <see cref="GraphAppearance.Current"/>'s <see cref="GraphAppearance.Mode"/>, so flipping the
/// toggle re-renders every graph in place — the same role the former <c>AdaptiveGraph</c> filled by
/// swapping its inner control.
/// </summary>
[ValueConversion(typeof(GraphRenderMode), typeof(DisplayMode))]
public sealed class GraphRenderModeToDisplayModeConverter : IValueConverter {
  /// <summary>
  /// Converts a <see cref="GraphRenderMode"/> to a <see cref="DisplayMode"/>.
  /// </summary>
  /// <param name="value">The GraphRenderMode value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted DisplayMode value.</returns>
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is GraphRenderMode.Dot ? DisplayMode.Dot : DisplayMode.Line;

  /// <summary>
  /// Converts a <see cref="DisplayMode"/> back to a <see cref="GraphRenderMode"/>.
  /// </summary>
  /// <param name="value">The DisplayMode value to convert.</param>
  /// <param name="targetType">The type of the target.</param>
  /// <param name="parameter">The converter parameter.</param>
  /// <param name="culture">The culture to use.</param>
  /// <returns>The converted GraphRenderMode value.</returns>
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
    value is DisplayMode.Dot ? GraphRenderMode.Dot : GraphRenderMode.Line;
}
