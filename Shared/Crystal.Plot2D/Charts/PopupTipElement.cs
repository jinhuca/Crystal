using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

public abstract class PopupTipElement : IPlotterElement {
  /// <summary>
  ///   Shows tooltips.
  /// </summary>
  private PopupTip popup;

  protected PopupTip GetPopupTipWindow() {
    if(popup != null) {
      return popup;
    }

    foreach(var item_ in Plotter.Children) {
      if(item_ is ViewportUIContainer) {
        var container_ = (ViewportUIContainer)item_;
        if(container_.Content is PopupTip) {
          return popup = (PopupTip)container_.Content;
        }
      }
    }

    popup = new PopupTip {
      Placement = PlacementMode.Relative,
      PlacementTarget = plotter.CentralGrid
    };
    Plotter.Children.Add(content: popup);
    return popup;
  }

  private void OnMouseLeave(object sender, MouseEventArgs e) {
    GetPopupTipWindow().Hide();
  }

  private void OnMouseMove(object sender, MouseEventArgs e) {
    var popup_ = GetPopupTipWindow();
    if(popup_.IsOpen) {
      popup_.Hide();
    }

    var screenPoint_ = e.GetPosition(relativeTo: plotter.CentralGrid);
    var viewportPoint_ = screenPoint_.ScreenToData(transform: plotter.Transform);

    var tooltip_ = GetTooltipForPoint();
    if(tooltip_ == null) {
      return;
    }

    popup_.VerticalOffset = screenPoint_.Y + 20;
    popup_.HorizontalOffset = screenPoint_.X;
    popup_.ShowDelayed(delay: TimeSpan.FromSeconds(value: 0));

    Grid grid_ = new();
    Rectangle rect_ = new() {
      Stroke = Brushes.Black,
      Fill = SystemColors.InfoBrush
    };

    StackPanel panel_ = new();
    panel_.Orientation = Orientation.Vertical;
    panel_.Children.Add(element: tooltip_);
    panel_.Margin = new Thickness(left: 4, top: 2, right: 4, bottom: 2);

    var textBlock_ = new TextBlock {
      Text = $"Location: {viewportPoint_.X:F2}, {viewportPoint_.Y:F2}",
      Foreground = SystemColors.GrayTextBrush
    };
    panel_.Children.Add(element: textBlock_);

    grid_.Children.Add(element: rect_);
    grid_.Children.Add(element: panel_);
    grid_.Measure(availableSize: SizeHelper.CreateInfiniteSize());
    popup_.Child = grid_;
  }

  protected virtual UIElement GetTooltipForPoint() {
    return null;
  }

  #region IPlotterElement Members

  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter) {
    this.plotter = (PlotterBase)plotter;
    plotter.CentralGrid.MouseMove += OnMouseMove;
    plotter.CentralGrid.MouseLeave += OnMouseLeave;
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    plotter.CentralGrid.MouseMove -= OnMouseMove;
    plotter.CentralGrid.MouseLeave -= OnMouseLeave;
    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  #endregion
}