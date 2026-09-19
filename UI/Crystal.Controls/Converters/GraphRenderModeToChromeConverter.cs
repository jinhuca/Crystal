using System;
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
public sealed class GraphRenderModeToChromeConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is not GraphRenderMode.Dot;

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}
