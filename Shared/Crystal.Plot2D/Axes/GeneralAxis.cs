using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Represents a base class for all axes.
/// Has several axis-specific and all WPF-specific properties.
/// </summary>
public abstract class GeneralAxis : ContentControl, IPlotterElement {
  /// <summary>
  /// Initializes a new instance of the <see cref="GeneralAxis"/> class.
  /// </summary>
  protected GeneralAxis() { }

  #region Placement property

  private AxisPlacement placement = AxisPlacement.Bottom;

  /// <summary>
  /// Gets or sets the placement of axis - place in Plotter where it should be placed.
  /// </summary>
  /// <value>The placement.</value>
  public AxisPlacement Placement {
    get => placement;
    set {
      if(placement != value) {
        ValidatePlacement(newPlacement: value);
        var oldPlacement = placement;
        placement = value;
        OnPlacementChanged(oldPlacement: oldPlacement, newPlacement: placement);
      }
    }
  }

  protected virtual void OnPlacementChanged(AxisPlacement oldPlacement, AxisPlacement newPlacement) { }

  protected Panel GetPanelByPlacement(AxisPlacement placement) {
    var panel = placement switch {
      AxisPlacement.Left => ParentPlotter.LeftPanel,
      AxisPlacement.Right => ParentPlotter.RightPanel,
      AxisPlacement.Top => ParentPlotter.TopPanel,
      AxisPlacement.Bottom => ParentPlotter.BottomPanel,
      _ => null
    };
    return panel;
  }

  /// <summary>
  /// Validates the placement - e.g., vertical axis should not be placed from top or bottom, etc.
  /// If proposed placement is wrong, throws an ArgumentException.
  /// </summary>
  /// <param name="newPlacement">The new placement.</param>
  protected virtual void ValidatePlacement(AxisPlacement newPlacement) { }

  #endregion

  protected void RaiseTicksChanged() {
    TicksChanged.Raise(sender: this);
  }

  public abstract void ForceUpdate();

  /// <summary>
  /// Occurs when ticks changes.
  /// </summary>
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public event EventHandler TicksChanged;

  /// <summary>
  /// Gets the screen coordinates of axis ticks.
  /// </summary>
  /// <value>The screen ticks.</value>
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public abstract double[] ScreenTicks { get; }

  /// <summary>
  /// Gets the screen coordinates of minor ticks.
  /// </summary>
  /// <value>The minor screen ticks.</value>
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public abstract MinorTickInfo<double>[] MinorScreenTicks { get; }

  #region IPlotterElement Members

  private PlotterBase plotter;
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public PlotterBase ParentPlotter => plotter;

  void IPlotterElement.OnPlotterAttached(PlotterBase thePlotter) {
    plotter = thePlotter;
    OnPlotterAttached(thePlotter: plotter);
  }

  protected virtual void OnPlotterAttached(PlotterBase thePlotter) { }

  void IPlotterElement.OnPlotterDetaching(PlotterBase thePlotter) {
    OnPlotterDetaching(thePlotter: plotter);
    plotter = null;
  }

  protected virtual void OnPlotterDetaching(PlotterBase thePlotter) { }

  public PlotterBase Plotter => plotter;

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
