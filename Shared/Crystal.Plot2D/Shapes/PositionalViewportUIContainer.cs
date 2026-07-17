#define old

using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Crystal.Plot2D.Shapes;

public sealed class PositionChangedEventArgs : EventArgs {
  public Point Position { get; internal set; }
  public Point PreviousPosition { get; internal set; }
}

public delegate Point PositionCoerceCallback(PositionalViewportUIContainer container, Point position);

public class PositionalViewportUIContainer : ContentControl, IPlotterElement {
  static PositionalViewportUIContainer() {
    var type = typeof(PositionalViewportUIContainer);

    // todo subscribe for properties changes
    HorizontalContentAlignmentProperty.AddOwner(ownerType: type, typeMetadata: new FrameworkPropertyMetadata(defaultValue: HorizontalAlignment.Center));
    VerticalContentAlignmentProperty.AddOwner(ownerType: type, typeMetadata: new FrameworkPropertyMetadata(defaultValue: VerticalAlignment.Center));
  }

  public PositionalViewportUIContainer() {
    PlotterEvents.PlotterChangedEvent.Subscribe(target: this, handler: OnPlotterChanged);

    //SetBinding(ViewportPanel.XProperty, new Binding("Position.X") { Source = this, Mode = BindingMode.TwoWay });
    //SetBinding(ViewportPanel.YProperty, new Binding("Position.Y") { Source = this, Mode = BindingMode.TwoWay });
  }

  protected void OnPlotterChanged(object sender, PlotterChangedEventArgs e) {
    if(e.CurrentPlotter != null) {
      OnPlotterAttached(plotter: e.CurrentPlotter);
    }
    else if(e.PreviousPlotter != null) {
      OnPlotterDetaching(plotter: e.PreviousPlotter);
    }
  }

  public Point Position {
    get => (Point)GetValue(dp: PositionProperty);
    set => SetValue(dp: PositionProperty, value: value);
  }

  public static readonly DependencyProperty PositionProperty =
    DependencyProperty.Register(
      name: nameof(Position),
      propertyType: typeof(Point),
      ownerType: typeof(PositionalViewportUIContainer),
      typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Point(x: 0, y: 0), propertyChangedCallback: OnPositionChanged, coerceValueCallback: CoercePosition));

  private static object CoercePosition(DependencyObject d, object value) {
    var owner = (PositionalViewportUIContainer)d;
    if(owner.positionCoerceCallbacks.Count > 0) {
      var position = (Point)value;
      foreach(var callback in owner.positionCoerceCallbacks) {
        position = callback(container: owner, position: position);
      }
      value = position;
    }
    return value;
  }

  private readonly ObservableCollection<PositionCoerceCallback> positionCoerceCallbacks = new();
  /// <summary>
  /// Gets the list of callbacks which are called every time Position changes to coerce it.
  /// </summary>
  /// <value>The position coerce callbacks.</value>
  public ObservableCollection<PositionCoerceCallback> PositionCoerceCallbacks => positionCoerceCallbacks;

  private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var container = (PositionalViewportUIContainer)d;
    container.OnPositionChanged(e: e);
  }

  public event EventHandler<PositionChangedEventArgs> PositionChanged;

  private void OnPositionChanged(DependencyPropertyChangedEventArgs e) {
    PositionChanged.Raise(sender: this, args: new PositionChangedEventArgs { Position = (Point)e.NewValue, PreviousPosition = (Point)e.OldValue });

    ViewportPanel.SetX(obj: this, value: Position.X);
    ViewportPanel.SetY(obj: this, value: Position.Y);
  }

  #region IPlotterElement Members

  private const string canvasName = "ViewportUIContainer_Canvas";
  private ViewportHostPanel hostPanel;
  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter) {
    if(Parent == null) {
      hostPanel = new ViewportHostPanel();
      Viewport2D.SetIsContentBoundsHost(obj: hostPanel, value: false);
      hostPanel.Children.Add(element: this);

      plotter.Dispatcher.BeginInvoke(method: () => {
        plotter.Children.Add(content: hostPanel);
      }, priority: DispatcherPriority.Send);
    }
#if !old
			Canvas hostCanvas = (Canvas)hostPanel.FindName(canvasName);
			if (hostCanvas == null)
			{
				hostCanvas = new Canvas { ClipToBounds = true };
				Panel.SetZIndex(hostCanvas, 1);

				INameScope nameScope = NameScope.GetNameScope(hostPanel);
				if (nameScope == null)
				{
					nameScope = new NameScope();
					NameScope.SetNameScope(hostPanel, nameScope);
				}

				hostPanel.RegisterName(canvasName, hostCanvas);
				hostPanel.Children.Add(hostCanvas);
			}

			hostCanvas.Children.Add(this);
#else
#endif

    var plotter2d = (PlotterBase)plotter;
    this.plotter = plotter2d;
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    var plotter2d = (PlotterBase)plotter;

#if !old
			Canvas hostCanvas = (Canvas)hostPanel.FindName(canvasName);

			if (hostCanvas.Children.Count == 1)
			{
				// only this ViewportUIContainer left
				hostPanel.Children.Remove(hostCanvas);
			}
			hostCanvas.Children.Remove(this);
#else
    if(hostPanel != null) {
      hostPanel.Children.Remove(element: this);
    }
    plotter.Dispatcher.BeginInvoke(method: () => {
      plotter.Children.Remove(item: hostPanel);
    }, priority: DispatcherPriority.Send);
#endif

    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
