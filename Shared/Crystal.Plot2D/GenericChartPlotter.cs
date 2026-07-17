using Crystal.Plot2D.Axes;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Transforms;
using System;
using System.Windows;

namespace Crystal.Plot2D;

public sealed class GenericChartPlotter<THorizontal, TVertical> {
  public AxisBase<THorizontal> HorizontalAxis { get; }

  private readonly AxisBase<TVertical> verticalAxis;
  public AxisBase<TVertical> VerticalAxis => verticalAxis;

  private readonly Plotter plotter;
  public Plotter Plotter => plotter;

  public Func<THorizontal, double> HorizontalToDoubleConverter => HorizontalAxis.ConvertToDouble;

  public Func<double, THorizontal> DoubleToHorizontalConverter => HorizontalAxis.ConvertFromDouble;

  public Func<TVertical, double> VerticalToDoubleConverter => verticalAxis.ConvertToDouble;

  public Func<double, TVertical> DoubleToVerticalConverter => verticalAxis.ConvertFromDouble;

  internal GenericChartPlotter(Plotter plotter) : this(plotter: plotter, horizontalAxis: plotter.MainHorizontalAxis as AxisBase<THorizontal>, verticalAxis: plotter.MainVerticalAxis as AxisBase<TVertical>) { }

  internal GenericChartPlotter(Plotter plotter, AxisBase<THorizontal> horizontalAxis, AxisBase<TVertical> verticalAxis) {
    HorizontalAxis = horizontalAxis ?? throw new ArgumentNullException(paramName: Strings.Exceptions.PlotterMainHorizontalAxisShouldNotBeNull);
    this.verticalAxis = verticalAxis ?? throw new ArgumentNullException(paramName: Strings.Exceptions.PlotterMainVerticalAxisShouldNotBeNull);
    this.plotter = plotter;
  }

  public GenericRect<THorizontal, TVertical> ViewportRect {
    get => CreateGenericRect(rect: plotter.Viewport.Visible);
    set => plotter.Viewport.Visible = CreateRect(value: value);
  }

  public GenericRect<THorizontal, TVertical> DataRect {
    get => CreateGenericRect(rect: plotter.Viewport.Visible.ViewportToData(transform: plotter.Viewport.Transform));
    set => plotter.Viewport.Visible = CreateRect(value: value).DataToViewport(transform: plotter.Viewport.Transform);
  }

  private DataRect CreateRect(GenericRect<THorizontal, TVertical> value) {
    var xMin = HorizontalToDoubleConverter(arg: value.XMin);
    var xMax = HorizontalToDoubleConverter(arg: value.XMax);
    var yMin = VerticalToDoubleConverter(arg: value.YMin);
    var yMax = VerticalToDoubleConverter(arg: value.YMax);

    return new DataRect(point1: new Point(x: xMin, y: yMin), point2: new Point(x: xMax, y: yMax));
  }

  private GenericRect<THorizontal, TVertical> CreateGenericRect(DataRect rect) {
    var xMin = rect.XMin;
    var xMax = rect.XMax;
    var yMin = rect.YMin;
    var yMax = rect.YMax;

    var res = new GenericRect<THorizontal, TVertical>(
      xMin: DoubleToHorizontalConverter(arg: xMin),
      yMin: DoubleToVerticalConverter(arg: yMin),
      xMax: DoubleToHorizontalConverter(arg: xMax),
      yMax: DoubleToVerticalConverter(arg: yMax));

    return res;
  }
}