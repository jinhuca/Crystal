using Crystal.Plot2D.Common;
using Crystal.Plot2D.Transforms;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Crystal.Plot2D.Axes;

/// <summary>
/// Represents a base class for all axes in Plotter.
/// Contains a real UI representation of axis - AxisControl, and means to adjust number of ticks, algorithms of their generating and 
/// look of ticks' labels.
/// </summary>
/// <typeparam name="T">Type of each tick's value</typeparam>
public abstract class AxisBase<T> : GeneralAxis, ITypedAxis<T>, IValueConversion<T> {
  /// <summary>
  /// Initializes a new instance of the <see cref="AxisBase&lt;T&gt;"/> class.
  /// </summary>
  /// <param name="axisControl">The axis control.</param>
  /// <param name="convertFromDouble">The convert from double.</param>
  /// <param name="convertToDouble">The convert to double.</param>
  protected AxisBase(AxisControl<T> axisControl, Func<double, T> convertFromDouble, Func<T, double> convertToDouble) {
    _convertToDouble = convertToDouble ?? throw new ArgumentNullException(paramName: nameof(convertToDouble));
    _convertFromDouble = convertFromDouble ?? throw new ArgumentNullException(paramName: nameof(convertFromDouble));
    _axisControl = axisControl ?? throw new ArgumentNullException(paramName: nameof(axisControl));

    axisControl.MakeDependent();
    axisControl.ConvertToDouble = convertToDouble;
    axisControl.ScreenTicksChanged += axisControl_ScreenTicksChanged;

    Content = axisControl;
    axisControl.SetBinding(dp: BackgroundProperty, binding: new Binding(path: nameof(Background)) { Source = this });
    Focusable = false;

    Loaded += OnLoaded;
  }

  public override void ForceUpdate() => _axisControl.UpdateUI();

  private void axisControl_ScreenTicksChanged(object sender, EventArgs e) => RaiseTicksChanged();

  /// <summary>
  /// Gets or sets a value indicating whether this axis is default axis.
  /// Plotter's AxisGrid gets axis ticks to display from two default axes - horizontal and vertical.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if this instance is default axis; otherwise, <c>false</c>.
  /// </value>
  public bool IsDefaultAxis {
    get => PlotterBase.GetIsDefaultAxis(obj: this);
    set => PlotterBase.SetIsDefaultAxis(obj: this, value: value);
  }

  private void OnLoaded(object sender, RoutedEventArgs e) => RaiseTicksChanged();

  private readonly AxisControl<T> _axisControl;

  /// <summary>
  /// Gets the screen coordinates of axis ticks.
  /// </summary>
  /// <value>The screen ticks.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public override double[] ScreenTicks => _axisControl.ScreenTicks;

  /// <summary>
  /// Gets the screen coordinates of minor ticks.
  /// </summary>
  /// <value>The minor screen ticks.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public override MinorTickInfo<double>[] MinorScreenTicks => _axisControl.MinorScreenTicks;

  /// <summary>
  /// Gets the axis control - actual UI representation of axis.
  /// </summary>
  /// <value>The axis control.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public AxisControl<T> AxisControl => _axisControl;

  /// <summary>
  /// Gets or sets the ticks provider, which is used to generate ticks in given range.
  /// </summary>
  /// <value>The ticks provider.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public ITicksProvider<T> TicksProvider {
    get => _axisControl.TicksProvider;
    set => _axisControl.TicksProvider = value;
  }

  /// <summary>
  /// Gets or sets the label provider, that is used to create UI look of axis ticks.
  /// 
  /// Should not be null.
  /// </summary>
  /// <value>The label provider.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [NotNull]
  public LabelProviderBase<T> LabelProvider {
    get => _axisControl.LabelProvider;
    set => _axisControl.LabelProvider = value;
  }

  /// <summary>
  /// Gets or sets the major label provider, which creates labels for major ticks.
  /// If null, major labels will not be shown.
  /// </summary>
  /// <value>The major label provider.</value>
  public LabelProviderBase<T> MajorLabelProvider {
    get => _axisControl.MajorLabelProvider;
    set => _axisControl.MajorLabelProvider = value;
  }

  /// <summary>
  /// Gets or sets the label string format, used to create simple formats of each tick's label, such as
  /// changing tick label from "1.2" to "$1.2".
  /// Should be in format "*{0}*", where '*' is any number of any chars.
  /// 
  /// If value is null, format string will not be used.
  /// </summary>
  /// <value>The label string format.</value>
  public string LabelStringFormat {
    get => LabelProvider.LabelStringFormat;
    set => LabelProvider.LabelStringFormat = value;
  }

  /// <summary>
  /// Gets or sets a value indicating whether to show minor ticks.
  /// </summary>
  /// <value><c>true</c> if show minor ticks; otherwise, <c>false</c>.</value>
  public bool ShowMinorTicks {
    get => _axisControl.DrawMinorTicks;
    set => _axisControl.DrawMinorTicks = value;
  }

  /// <summary>
  /// Gets or sets a value indicating whether to show major labels.
  /// </summary>
  /// <value><c>true</c> if show major labels; otherwise, <c>false</c>.</value>
  public bool ShowMajorLabels {
    get => _axisControl.DrawMajorLabels;
    set => _axisControl.DrawMajorLabels = value;
  }

  protected override void OnPlotterAttached(PlotterBase thePlotter) {
    thePlotter.Viewport.PropertyChanged += OnViewportPropertyChanged;

    var panel = GetPanelByPlacement(placement: Placement);
    if(panel != null) {
      var index = GetInsertionIndexByPlacement(placement: Placement, panel: panel);
      panel.Children.Insert(index: index, element: this);
    }

    using(_axisControl.OpenUpdateRegion(forceUpdate: true)) {
      UpdateAxisControl(plotter2d: thePlotter);
    }
  }

  private void UpdateAxisControl(PlotterBase plotter2d) {
    _axisControl.Transform = plotter2d.Viewport.Transform;
    _axisControl.Range = CreateRangeFromRect(visible: plotter2d.Visible.ViewportToData(transform: plotter2d.Viewport.Transform));
  }

  private int GetInsertionIndexByPlacement(AxisPlacement placement, Panel panel) {
    var index = panel.Children.Count;

    switch(placement) {
      case AxisPlacement.Left:
        index = 0;
        break;
      case AxisPlacement.Top:
        index = 0;
        break;
    }

    return index;
  }

  private ExtendedPropertyChangedEventArgs visibleChangedEventArgs;
  private int viewportPropertyChangedEnters;
  private DataRect prevDataRect = DataRect.Empty;
  private void OnViewportPropertyChanged(object sender, ExtendedPropertyChangedEventArgs e) {
    if(viewportPropertyChangedEnters > 4) {
      if(e.PropertyName == "Visible") {
        visibleChangedEventArgs = e;
      }
      return;
    }

    viewportPropertyChangedEnters++;

    var viewport = (Viewport2D)sender;

    var visible = viewport.Visible;

    var dataRect = visible.ViewportToData(transform: viewport.Transform);
    var forceUpdate = dataRect != prevDataRect;
    prevDataRect = dataRect;

    var range = CreateRangeFromRect(visible: dataRect);

    using(_axisControl.OpenUpdateRegion(forceUpdate: false)) // todo was forceUpdate
    {
      _axisControl.Range = range;
      _axisControl.Transform = viewport.Transform;
    }

    Dispatcher.BeginInvoke(method: () => {
      viewportPropertyChangedEnters--;
      if(visibleChangedEventArgs != null) {
        OnViewportPropertyChanged(sender: Plotter.Viewport, e: visibleChangedEventArgs);
      }
      visibleChangedEventArgs = null;
    }, priority: DispatcherPriority.Render);
  }

  private Func<double, T> _convertFromDouble;
  /// <summary>
  /// Gets or sets the delegate that is used to create each tick from double.
  /// Is used to create typed range to display for internal AxisControl.
  /// If changed, ConvertToDouble should be changed appropriately, too.
  /// Should not be null.
  /// </summary>
  /// <value>The convert from double.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [NotNull]
  public Func<double, T> ConvertFromDouble {
    get => _convertFromDouble;
    set {
      ArgumentNullException.ThrowIfNull(value);

      if(_convertFromDouble != value) {
        _convertFromDouble = value;
        if(ParentPlotter != null) {
          UpdateAxisControl(plotter2d: ParentPlotter);
        }
      }
    }
  }

  private Func<T, double> _convertToDouble;

  /// <summary>
  /// Gets or sets the delegate that is used to convert each tick to double.
  /// Is used by internal AxisControl to convert tick to double to get tick's coordinates inside of viewport.
  /// If changed, ConvertFromDouble should be changed appropriately, too.
  /// Should not be null.
  /// </summary>
  /// <value>The convert to double.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [NotNull]
  public Func<T, double> ConvertToDouble {
    get => _convertToDouble;
    set {
      ArgumentNullException.ThrowIfNull(value);
      if(_convertToDouble == value) return;
      _convertToDouble = value;
      _axisControl.ConvertToDouble = value;
    }
  }

  /// <summary>
  /// Sets conversions of axis - functions used to convert values of axis type to and from double values of viewport.
  /// Sets both ConvertToDouble and ConvertFromDouble properties.
  /// </summary>
  /// <param name="min">The minimal viewport value.</param>
  /// <param name="minValue">The value of axis type, corresponding to minimal viewport value.</param>
  /// <param name="max">The maximal viewport value.</param>
  /// <param name="maxValue">The value of axis type, corresponding to maximal viewport value.</param>
  public virtual void SetConversion(double min, T minValue, double max, T maxValue) {
    throw new NotImplementedException();
  }

  private Range<T> CreateRangeFromRect(DataRect visible) {
    T min, max;

    switch(Placement) {
      case AxisPlacement.Left:
      case AxisPlacement.Right:
        min = ConvertFromDouble(arg: visible.YMin);
        max = ConvertFromDouble(arg: visible.YMax);
        break;
      case AxisPlacement.Top:
      case AxisPlacement.Bottom:
        min = ConvertFromDouble(arg: visible.XMin);
        max = ConvertFromDouble(arg: visible.XMax);
        break;
      default:
        throw new NotSupportedException();
    }

    TrySort(min: ref min, max: ref max);
    var range = new Range<T>(min: min, max: max);
    return range;
  }

  private static void TrySort<TS>(ref TS min, ref TS max) {
    if(min is IComparable) {
      var c1 = (IComparable)min;
      // if min > max
      if(c1.CompareTo(obj: max) > 0) {
        var temp = min;
        min = max;
        max = temp;
      }
    }
  }

  protected override void OnPlacementChanged(AxisPlacement oldPlacement, AxisPlacement newPlacement) {
    _axisControl.Placement = Placement;
    if(ParentPlotter != null) {
      var panel = GetPanelByPlacement(placement: oldPlacement);
      panel.Children.Remove(element: this);

      var newPanel = GetPanelByPlacement(placement: newPlacement);
      var index = GetInsertionIndexByPlacement(placement: newPlacement, panel: newPanel);
      newPanel.Children.Insert(index: index, element: this);
    }
  }

  protected override void OnPlotterDetaching(PlotterBase thePlotter) {
    if(thePlotter == null) {
      return;
    }

    var panel = GetPanelByPlacement(placement: Placement);
    if(panel != null) {
      panel.Children.Remove(element: this);
    }

    thePlotter.Viewport.PropertyChanged -= OnViewportPropertyChanged;
    _axisControl.Transform = null;
  }
}
