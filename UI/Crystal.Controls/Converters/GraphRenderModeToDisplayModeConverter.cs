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
public sealed class GraphRenderModeToDisplayModeConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is GraphRenderMode.Dot ? DisplayMode.Dot : DisplayMode.Line;

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is DisplayMode.Dot ? GraphRenderMode.Dot : GraphRenderMode.Line;
}
