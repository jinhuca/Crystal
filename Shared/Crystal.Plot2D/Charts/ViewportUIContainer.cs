using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Crystal.Plot2D.Charts;

[ContentProperty(name: "Content")]
public sealed class ViewportUIContainer : DependencyObject, IPlotterElement {
  public ViewportUIContainer() { }

  public ViewportUIContainer(FrameworkElement content) {
    Content = content;
  }

  private FrameworkElement content;
  [NotNull]
  public FrameworkElement Content {
    get => content;
    set => content = value;
  }

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) {
    this.plotter = plotter;
    if(Content == null) {
      return;
    }

    var plotterPanel_ = GetPlotterPanel(obj: Content);
    //Plotter.SetPlotter(Content, _plotter);

    if(plotterPanel_ == PlotterPanel.MainCanvas) {
      // if all four Canvas.{Left|Right|Top|Bottom} properties are not set,
      // and as we are adding by default content to MainCanvas, 
      // and since I like more when buttons are by default in right down corner - 
      // set bottom and right to 10.
      var left_ = Canvas.GetLeft(element: content);
      var top_ = Canvas.GetTop(element: content);
      var bottom_ = Canvas.GetBottom(element: content);
      var right_ = Canvas.GetRight(element: content);

      if(left_.IsNaN() && right_.IsNaN() && bottom_.IsNaN() && top_.IsNaN()) {
        Canvas.SetBottom(element: content, length: 10.0);
        Canvas.SetRight(element: content, length: 10.0);
      }
      plotter.MainCanvas.Children.Add(element: Content);
    }
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) {
    if(Content != null) {
      var plotterPanel_ = GetPlotterPanel(obj: Content);
      //Plotter.SetPlotter(Content, null);
      if(plotterPanel_ == PlotterPanel.MainCanvas) {
        plotter.MainCanvas.Children.Remove(element: Content);
      }
    }

    this.plotter = null;
  }

  private PlotterBase plotter;
  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion

  [AttachedPropertyBrowsableForChildren]
  public static PlotterPanel GetPlotterPanel(DependencyObject obj)
    => (PlotterPanel)obj.GetValue(dp: PlotterPanelProperty);

  public static void SetPlotterPanel(DependencyObject obj, PlotterPanel value)
    => obj.SetValue(dp: PlotterPanelProperty, value: value);

  public static readonly DependencyProperty PlotterPanelProperty = DependencyProperty.RegisterAttached(
    name: "PlotterPanel",
    propertyType: typeof(PlotterPanel),
    ownerType: typeof(ViewportUIContainer),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: PlotterPanel.MainCanvas));
}