using Crystal.Plot2D.Axes;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Represents a title of horizontal axis. Can be placed from top or bottom of Plotter.
/// </summary>
public sealed class HorizontalAxisTitle : ContentControl, IPlotterElement {
  public HorizontalAxisTitle() {
    FontSize = 16;
    HorizontalAlignment = HorizontalAlignment.Center;
  }

  public HorizontalAxisTitle(object content) : this() => Content = content;

  public PlotterBase Plotter { get; private set; }

  public void OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    AddToPlotter();
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    RemoveFromPlotter();
    Plotter = null;
  }

  private Panel GetHostPanel(PlotterBase plotter)
    => placement == AxisPlacement.Bottom ? plotter.BottomPanel : plotter.TopPanel;

  private int GetInsertPosition(Panel panel)
    => placement == AxisPlacement.Bottom ? panel.Children.Count : 0;

  private AxisPlacement placement = AxisPlacement.Bottom;

  public AxisPlacement Placement {
    get => placement;
    set {
      if(!value.IsBottomOrTop()) {
        throw new ArgumentException(message:
          $"HorizontalAxisTitle only supports Top and Bottom values of AxisPlacement, you passed '{value}'",
          paramName: nameof(Placement));
      }

      if(placement == value) return;
      if(Plotter != null) {
        RemoveFromPlotter();
      }
      placement = value;
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