using Crystal.Plot2D.Axes;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Represents a title of vertical axis. Can be placed from left or right of Plotter.
/// </summary>
public sealed class VerticalAxisTitle : ContentControl, IPlotterElement {
  public VerticalAxisTitle() {
    FontSize = 16;
    VerticalAlignment = VerticalAlignment.Center;
    ChangeLayoutTransform();
  }

  public VerticalAxisTitle(object content) : this() => Content = content;

  public PlotterBase Plotter { get; private set; }

  public void OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    AddToPlotter();
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    Plotter = null;
    RemoveFromPlotter();
  }

  private void ChangeLayoutTransform() {
    LayoutTransform = placement == AxisPlacement.Left
      ? new RotateTransform(angle: -90)
      : new RotateTransform(angle: 90);
  }

  private Panel GetHostPanel(PlotterBase plotter)
    => placement == AxisPlacement.Left ? plotter.LeftPanel : plotter.RightPanel;

  private int GetInsertPosition(Panel panel)
    => placement == AxisPlacement.Left ? 0 : panel.Children.Count;

  private AxisPlacement placement = AxisPlacement.Left;

  public AxisPlacement Placement {
    get => placement;
    set {
      if(value.IsBottomOrTop()) {
        throw new ArgumentException(
          message: $"VerticalAxisTitle only supports Left and Right values of AxisPlacement, you passed '{value}'",
          paramName: nameof(Placement));
      }

      if(placement == value) return;
      if(Plotter != null) {
        RemoveFromPlotter();
      }
      placement = value;
      ChangeLayoutTransform();
      if(Plotter != null) {
        AddToPlotter();
      }
    }
  }

  private void AddToPlotter() {
    var hostPanel_ = GetHostPanel(plotter: Plotter);
    var index_ = GetInsertPosition(panel: hostPanel_);
    hostPanel_.Children.Insert(index: index_, element: this);
  }

  private void RemoveFromPlotter() {
    var hostPanel_ = GetHostPanel(plotter: Plotter);
    hostPanel_.Children.Remove(element: this);
  }
}