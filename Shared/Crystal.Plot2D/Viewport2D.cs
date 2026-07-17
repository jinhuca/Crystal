using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using Crystal.Plot2D.ViewportConstraints;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D;

/// <summary>
///   Viewport2D provides virtual coordinates.
/// </summary>
public class Viewport2D : DependencyObject {
  /// <summary>
  /// This counter is a part of workaround for endless axis resize loop. 
  /// It is reset on Viewport property assignment and on entire plotter resize.
  /// </summary>
  internal int UpdateIterationCount { get; set; }

  internal PlotterBase PlotterBase { get; }

  internal FrameworkElement HostElement { get; }

  protected internal Viewport2D(FrameworkElement host, PlotterBase plotter) {
    HostElement = host;
    host.ClipToBounds = true;
    host.SizeChanged += OnHostElementSizeChanged;

    PlotterBase = plotter;
    plotter.Children.CollectionChanged += OnPlotterChildrenChanged;
    constraints = new ConstraintCollection(viewport: this);
    constraints.Add(item: new MinimalSizeConstraint());
    constraints.CollectionChanged += constraints_CollectionChanged;

    fitToViewConstraints = new ConstraintCollection(viewport: this);
    fitToViewConstraints.CollectionChanged += fitToViewConstraints_CollectionChanged;
    readonlyContentBoundsHosts = new ReadOnlyObservableCollection<DependencyObject>(list: contentBoundsHosts);
    UpdateVisible();
    UpdateTransform();
  }

  private void OnHostElementSizeChanged(object sender, SizeChangedEventArgs e) {
    SetValue(key: OutputPropertyKey, value: new Rect(size: e.NewSize));
    CoerceValue(dp: VisibleProperty);
  }

  private void fitToViewConstraints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    if(IsFittedToView) {
      CoerceValue(dp: VisibleProperty);
    }
  }

  private void constraints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    CoerceValue(dp: VisibleProperty);
  }

  private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var viewport = (Viewport2D)d;
    // This counter is a part of workaround for endless axis resize loop
    // If internal update count exceeds threshold stop enforcing restrictions
    if(e.Property == VisibleProperty) {
      if(viewport.UpdateIterationCount++ > 8) {
        viewport.EnforceRestrictions = false;
        Debug.WriteLine(message: "Plotter: update cycle detected. Viewport constraints disabled.");
      }
    }

    viewport.UpdateTransform();
    viewport.RaisePropertyChangedEvent(e: e);
  }

  public BindingExpressionBase SetBinding(DependencyProperty property, BindingBase binding) {
    return BindingOperations.SetBinding(target: this, dp: property, binding: binding);
  }

  /// <summary>
  ///   Forces viewport to go to fit to view mode - clears locally set value of <see cref="Visible"/> property
  ///   and sets it during the coercion process to a value of united content bounds of all charts inside of <see cref="Plotter"/>.
  /// </summary>
  public void FitToView() {
    if(!IsFittedToView) {
      ClearValue(dp: VisibleProperty);
      CoerceValue(dp: VisibleProperty);
    }
  }

  /// <summary>
  ///   Gets a value indicating whether Viewport is fitted to view.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if Viewport is fitted to view; otherwise, <c>false</c>.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public bool IsFittedToView => ReadLocalValue(dp: VisibleProperty) == DependencyProperty.UnsetValue;

  internal void UpdateVisible() {
    //if (updateVisibleOperation == null)
    //{
    //    updateVisibleOperation = Dispatcher.BeginInvoke(() => UpdateVisible(), DispatcherPriority.Normal);
    //    return;
    //}

    //updateVisibleOperation = Dispatcher.BeginInvoke(() =>
    //{
    //    updateVisibleOperation = null;

    if(IsFittedToView) {
      CoerceValue(dp: VisibleProperty);
    }
    //}, DispatcherPriority.Normal);
  }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public PlotterBase Plotter => PlotterBase;

  private readonly ConstraintCollection constraints;

  /// <summary>
  ///   Gets the collection of <see cref="ViewportConstraint"/>s that are applied each time <see cref="Visible"/> is updated.
  /// </summary>
  /// <value>
  ///   The constraints.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public ConstraintCollection Constraints => constraints;

  private readonly ConstraintCollection fitToViewConstraints;

  private bool enforceConstraints = true;

  public bool EnforceRestrictions {
    get => enforceConstraints;
    set => enforceConstraints = value;
  }

  /// <summary>
  ///   Gets the collection of <see cref="ViewportConstraint"/>s that are applied only when Viewport is fitted to view.
  /// </summary>
  /// <value>
  ///   The fit to view constraints.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public ConstraintCollection FitToViewConstraints => fitToViewConstraints;

  #region Output property

  /// <summary>
  ///   Gets the rectangle in screen coordinates that is output.
  /// </summary>
  /// <value>
  ///   The output.
  /// </value>
  public Rect Output => (Rect)GetValue(dp: OutputProperty);

  [SuppressMessage(category: "Microsoft.Security", checkId: "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
  private static readonly DependencyPropertyKey OutputPropertyKey = DependencyProperty.RegisterReadOnly(
    name: "Output",
    propertyType: typeof(Rect),
    ownerType: typeof(Viewport2D),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Rect(x: 0, y: 0, width: 1, height: 1), propertyChangedCallback: OnPropertyChanged));

  /// <summary>
  ///   Identifies the <see cref="Output"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty OutputProperty = OutputPropertyKey.DependencyProperty;

  #endregion

  #region UnitedContentBounds property

  /// <summary>
  ///   Gets the united content bounds of all the charts.
  /// </summary>
  /// <value>
  ///   The content bounds.
  /// </value>
  public DataRect UnitedContentBounds {
    get => (DataRect)GetValue(dp: UnitedContentBoundsProperty);
    internal set => SetValue(dp: UnitedContentBoundsProperty, value: value);
  }

  /// <summary>
  ///   Identifies the <see cref="UnitedContentBounds"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty UnitedContentBoundsProperty = DependencyProperty.Register(
    name: nameof(UnitedContentBounds),
    propertyType: typeof(DataRect),
    ownerType: typeof(Viewport2D),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: DataRect.Empty, propertyChangedCallback: OnUnitedContentBoundsChanged));

  private static void OnUnitedContentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var owner = (Viewport2D)d;
    owner.ContentBoundsChanged.Raise(sender: owner);
  }

  public event EventHandler ContentBoundsChanged;

  #endregion

  #region Visible property

  /// <summary>
  ///   Gets or sets the visible rectangle.
  /// </summary>
  /// <value>
  ///   The visible.
  /// </value>
  public DataRect Visible {
    get => (DataRect)GetValue(dp: VisibleProperty);
    set {
      // This code is a part of workaround for endless axis resize loop
      UpdateIterationCount = 0;
      if(!EnforceRestrictions) {
        Debug.WriteLine(message: "Plotter: enabling viewport constraints");
        EnforceRestrictions = true;
      }
      SetValue(dp: VisibleProperty, value: value);
    }
  }

  /// <summary>
  ///   Identifies the <see cref="Visible"/> dependency property.
  /// </summary>
  public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
    name: nameof(Visible),
    propertyType: typeof(DataRect),
    ownerType: typeof(Viewport2D),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new DataRect(xMin: 0, yMin: 0, width: 1, height: 1), propertyChangedCallback: OnPropertyChanged, coerceValueCallback: OnCoerceVisible),
    validateValueCallback: ValidateVisibleCallback);

  private static bool ValidateVisibleCallback(object value) {
    var rect = (DataRect)value;
    return !rect.IsNaN();
  }

  private void UpdateContentBoundsHosts() {
    contentBoundsHosts.Clear();
    foreach(var item in PlotterBase.Children) {
      if(item is DependencyObject dependencyObject) {
        var hasNonEmptyBounds = !GetContentBounds(obj: dependencyObject).IsEmpty;
        if(hasNonEmptyBounds && GetIsContentBoundsHost(obj: dependencyObject)) {
          contentBoundsHosts.Add(item: dependencyObject);
        }
      }
    }

    UpdateVisible();
  }

  private readonly ObservableCollection<DependencyObject> contentBoundsHosts = new();
  private readonly ReadOnlyObservableCollection<DependencyObject> readonlyContentBoundsHosts;

  /// <summary>
  ///   Gets the collection of all charts that can has its own content bounds.
  /// </summary>
  /// <value>
  ///   The content bounds hosts.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public ReadOnlyObservableCollection<DependencyObject> ContentBoundsHosts => readonlyContentBoundsHosts;

  private bool useApproximateContentBoundsComparison = true;

  /// <summary>
  ///   Gets or sets a value indicating whether to use approximate content bounds comparison.
  ///   This this property to true can increase performance, as Visible will change less often.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if approximate content bounds comparison is used; otherwise, <c>false</c>.
  /// </value>
  public bool UseApproximateContentBoundsComparison {
    get => useApproximateContentBoundsComparison;
    set => useApproximateContentBoundsComparison = value;
  }

  private double maxContentBoundsComparisonMistake = 0.02;
  public double MaxContentBoundsComparisonMistake {
    get => maxContentBoundsComparisonMistake;
    set => maxContentBoundsComparisonMistake = value;
  }

  private DataRect prevContentBounds = DataRect.Empty;
  protected virtual DataRect CoerceVisible(DataRect newVisible) {
    if(Plotter == null) {
      return newVisible;
    }

    var isDefaultValue = newVisible == (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
    if(isDefaultValue) {
      newVisible = DataRect.Empty;
    }

    if(isDefaultValue && IsFittedToView) {
      // determining content bounds
      var bounds = DataRect.Empty;

      foreach(var item in contentBoundsHosts) {
        if(item is not IPlotterElement plotterElement) {
          continue;
        }

        if(plotterElement.Plotter == null) {
          continue;
        }

        var plotter = (PlotterBase)plotterElement.Plotter;
        var visual = plotter.VisualBindings[element: plotterElement];
        if(visual.Visibility == Visibility.Visible) {
          var contentBounds = GetContentBounds(obj: item);
          if(contentBounds.Width.IsNaN() || contentBounds.Height.IsNaN()) {
            continue;
          }

          bounds.UnionFinite(rect: contentBounds);
        }
      }

      if(useApproximateContentBoundsComparison) {
        var intersection = prevContentBounds;
        intersection.Intersect(rect: bounds);

        var currSquare = bounds.GetSquare();
        var prevSquare = prevContentBounds.GetSquare();
        var intersectionSquare = intersection.GetSquare();
        var squareTopLimit = 1 + maxContentBoundsComparisonMistake;
        var squareBottomLimit = 1 - maxContentBoundsComparisonMistake;

        if(intersectionSquare != 0) {
          var currRatio = currSquare / intersectionSquare;
          var prevRatio = prevSquare / intersectionSquare;

          if(squareBottomLimit < currRatio && currRatio < squareTopLimit && squareBottomLimit < prevRatio && prevRatio < squareTopLimit) {
            bounds = prevContentBounds;
          }
        }
      }

      prevContentBounds = bounds;
      UnitedContentBounds = bounds;

      // applying fit-to-view constraints
      bounds = fitToViewConstraints.Apply(oldVisible: Visible, newVisible: bounds, viewport: this);

      // enlarging
      if(!bounds.IsEmpty) {
        bounds = CoordinateUtilities.RectZoom(rect: bounds, zoomCenter: bounds.GetCenter(), ratio: clipToBoundsEnlargeFactor);
      }
      else {
        bounds = (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
      }
      newVisible.Union(rect: bounds);
    }

    if(newVisible.IsEmpty) {
      newVisible = (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
    }
    else if(newVisible.Width == 0 || newVisible.Height == 0) {
      var defRect = (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
      var size = newVisible.Size;
      var location = newVisible.Location;

      if(newVisible.Width == 0) {
        size.Width = defRect.Width;
        location.X -= size.Width / 2;
      }
      if(newVisible.Height == 0) {
        size.Height = defRect.Height;
        location.Y -= size.Height / 2;
      }

      newVisible = new DataRect(location: location, size: size);
    }

    // apply domain constraint
    newVisible = domainConstraint.Apply(oldDataRect: Visible, newDataRect: newVisible, viewport: this);

    // apply other restrictions
    if(enforceConstraints) {
      newVisible = constraints.Apply(oldVisible: Visible, newVisible: newVisible, viewport: this);
    }

    // applying transform's data domain constraint
    if(!transform.DataTransform.DataDomain.IsEmpty) {
      var newDataRect = newVisible.ViewportToData(transform: transform);
      newDataRect = DataRect.Intersect(rect1: newDataRect, rect2: transform.DataTransform.DataDomain);
      newVisible = newDataRect.DataToViewport(transform: transform);
    }

    if(newVisible.IsEmpty) {
      newVisible = new Rect(x: 0, y: 0, width: 1, height: 1);
    }

    return newVisible;
  }

  private static object OnCoerceVisible(DependencyObject d, object newValue) {
    var viewport = (Viewport2D)d;
    var newRect = viewport.CoerceVisible(newVisible: (DataRect)newValue);
    if(newRect.Width == 0 || newRect.Height == 0) {
      // doesn't apply rects with zero square
      return DependencyProperty.UnsetValue;
    }

    return newRect;
  }

  #endregion

  #region Domain

  private readonly DomainConstraint domainConstraint = new() { Domain = Rect.Empty };

  /// <summary>
  ///   Gets or sets the domain - rectangle in viewport coordinates that limits maximal size of <see cref="Visible"/> rectangle.
  /// </summary>
  /// <value>
  ///   The domain.
  /// </value>
  public DataRect Domain {
    get => (DataRect)GetValue(dp: DomainProperty);
    set => SetValue(dp: DomainProperty, value: value);
  }

  public static readonly DependencyProperty DomainProperty = DependencyProperty.Register(
    name: nameof(Domain),
    propertyType: typeof(DataRect),
    ownerType: typeof(Viewport2D),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: DataRect.Empty, propertyChangedCallback: OnDomainReplaced));

  private static void OnDomainReplaced(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var owner = (Viewport2D)d;
    owner.OnDomainChanged();
  }

  private void OnDomainChanged() {
    domainConstraint.Domain = Domain;
    DomainChanged.Raise(sender: this);
    CoerceValue(dp: VisibleProperty);
  }

  /// <summary>
  ///   Occurs when <see cref="Domain"/> property changes.
  /// </summary>
  public event EventHandler DomainChanged;

  #endregion

  private double clipToBoundsEnlargeFactor = 1.10;

  /// <summary>
  ///   Gets or sets the viewport enlarge factor.
  /// </summary>
  /// <remarks>
  ///   Default value is 1.10.
  /// </remarks>
  /// <value>The clip to bounds factor.</value>
  public double ClipToBoundsEnlargeFactor {
    get => clipToBoundsEnlargeFactor;
    set {
      if(clipToBoundsEnlargeFactor != value) {
        clipToBoundsEnlargeFactor = value;
        UpdateVisible();
      }
    }
  }

  private void UpdateTransform() {
    transform = transform.WithRects(visibleRect: Visible, screenRect: Output);
  }

  private CoordinateTransform transform = CoordinateTransform.CreateDefault();

  /// <summary>
  ///   Gets or sets the coordinate transform of Viewport.
  /// </summary>
  /// <value>
  ///   The transform.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [Common.NotNull]
  public virtual CoordinateTransform Transform {
    get => transform;
    set {
      value.VerifyNotNull();
      if(value != transform) {
        var oldTransform = transform;
        transform = value;
        RaisePropertyChangedEvent(propertyName: nameof(Transform), oldValue: oldTransform, newValue: transform);
      }
    }
  }

  /// <summary>
  ///   Occurs when viewport property changes.
  /// </summary>
  public event EventHandler<ExtendedPropertyChangedEventArgs> PropertyChanged;

  private void RaisePropertyChangedEvent(string propertyName, object oldValue, object newValue) {
    if(PropertyChanged != null) {
      RaisePropertyChanged(args: new ExtendedPropertyChangedEventArgs { PropertyName = propertyName, OldValue = oldValue, NewValue = newValue });
    }
  }

  private void RaisePropertyChangedEvent(string propertyName) {
    if(PropertyChanged != null) {
      RaisePropertyChanged(args: new ExtendedPropertyChangedEventArgs { PropertyName = propertyName });
    }
  }

  private void RaisePropertyChangedEvent(DependencyPropertyChangedEventArgs e) {
    if(PropertyChanged != null) {
      RaisePropertyChanged(args: ExtendedPropertyChangedEventArgs.FromDependencyPropertyChanged(e: e));
    }
  }

  //private DispatcherOperation pendingRaisePropertyChangedOperation;
  //private bool inRaisePropertyChanged = false;
  protected virtual void RaisePropertyChanged(ExtendedPropertyChangedEventArgs args) {
    //if (inRaisePropertyChanged)
    //{
    //    if (pendingRaisePropertyChangedOperation != null)
    //        pendingRaisePropertyChangedOperation.Abort();
    //    pendingRaisePropertyChangedOperation = Dispatcher.BeginInvoke(() => RaisePropertyChanged(args), DispatcherPriority.Normal);
    //    return;
    //}

    //pendingRaisePropertyChangedOperation = null;
    //inRaisePropertyChanged = true;

    PropertyChanged.Raise(sender: this, args: args);

    //inRaisePropertyChanged = false;
  }

  private void OnPlotterChildrenChanged(object sender, NotifyCollectionChangedEventArgs e) {
    UpdateContentBoundsHosts();
  }

  #region Panning state

  private Viewport2DPanningState panningState = Viewport2DPanningState.NotPanning;
  public Viewport2DPanningState PanningState {
    get => panningState;
    set {
      var prevState = panningState;
      panningState = value;
      OnPanningStateChanged(prevState: prevState, currState: panningState);
    }
  }

  private void OnPanningStateChanged(Viewport2DPanningState prevState, Viewport2DPanningState currState) {
    PanningStateChanged.Raise(sender: this, prevValue: prevState, currValue: currState);
    if(currState == Viewport2DPanningState.Panning) {
      BeginPanning.Raise(sender: this);
    }
    else if(currState == Viewport2DPanningState.NotPanning) {
      EndPanning.Raise(sender: this);
    }
  }

  internal event EventHandler<ValueChangedEventArgs<Viewport2DPanningState>> PanningStateChanged;

  public event EventHandler BeginPanning;
  public event EventHandler EndPanning;

  #endregion // end of Panning state


  #region Attached Properties

  #region IsContentBoundsHost attached property

  public static bool GetIsContentBoundsHost(DependencyObject obj) => (bool)obj.GetValue(dp: IsContentBoundsHostProperty);

  public static void SetIsContentBoundsHost(DependencyObject obj, bool value) => obj.SetValue(dp: IsContentBoundsHostProperty, value: value);

  public static readonly DependencyProperty IsContentBoundsHostProperty = DependencyProperty.RegisterAttached(
    name: "IsContentBoundsHost",
    propertyType: typeof(bool),
    ownerType: typeof(Viewport2D),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: OnIsContentBoundsChanged));

  private static void OnIsContentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if(d is IPlotterElement plotterElement && plotterElement.Plotter != null) {
      var plotter2d = plotterElement.Plotter;
      plotter2d.Viewport.UpdateContentBoundsHosts();
    }
  }

  #endregion

  #region ContentBounds attached property

  public static DataRect GetContentBounds(DependencyObject obj) => (DataRect)obj.GetValue(dp: ContentBoundsProperty);

  public static void SetContentBounds(DependencyObject obj, DataRect value) => obj.SetValue(dp: ContentBoundsProperty, value: value);

  public static readonly DependencyProperty ContentBoundsProperty = DependencyProperty.RegisterAttached(
    name: "ContentBounds",
    propertyType: typeof(DataRect),
    ownerType: typeof(Viewport2D),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: DataRect.Empty, propertyChangedCallback: OnContentBoundsChanged, coerceValueCallback: CoerceContentBounds));

  private static object CoerceContentBounds(DependencyObject d, object value) {
    var prevBounds = GetContentBounds(obj: d);
    var currBounds = (DataRect)value;
    var approximateComparanceAllowed = GetUsesApproximateContentBoundsComparison(obj: d);
    var areClose = approximateComparanceAllowed && currBounds.IsCloseTo(rect2: prevBounds, difference: 0.005);
    return areClose ? DependencyProperty.UnsetValue : value;
  }

  private static void OnContentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if(d is IPlotterElement element) {
      if(element is FrameworkElement frElement) {
        frElement.RaiseEvent(e: new RoutedEventArgs(routedEvent: ContentBoundsChangedEvent));
      }

      if(element.Plotter is PlotterBase plotter2d) {
        plotter2d.Viewport.UpdateContentBoundsHosts();
      }
    }
  }

  public static readonly RoutedEvent ContentBoundsChangedEvent = EventManager.RegisterRoutedEvent(
    name: "ContentBoundsChanged",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(RoutedEventHandler),
    ownerType: typeof(Viewport2D));

  #endregion

  #region UsesApproximateContentBoundsComparison

  /// <summary>
  ///   Gets a value indicating whether approximate content bounds comparison will be used while deciding whether to updating Viewport2D.ContentBounds
  ///   attached dependency property or not.
  ///   Approximate content bounds comparison can make Viewport's Visible to changed less frequent, but it can lead to
  ///   some bugs if content bounds are large but visible area is not compared to them.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if approximate content bounds comparison is used while deciding whether to set new value of content bounds, or not; otherwise, <c>false</c>.
  /// </value>
  public static bool GetUsesApproximateContentBoundsComparison(DependencyObject obj) {
    return (bool)obj.GetValue(dp: UsesApproximateContentBoundsComparisonProperty);
  }

  /// <summary>
  /// Sets a value indicating whether approximate content bounds comparison will be used while deciding whether to updating Viewport2D.ContentBounds
  /// attached dependency property or not.
  /// Approximate content bounds comparison can make Viewport's Visible to changed less frequent, but it can lead to
  /// some bugs if content bounds are large but visible area is not compared to them.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if approximate content bounds comparison is used while deciding whether to set new value of content bounds, or not; otherwise, <c>false</c>.
  /// </value>		
  public static void SetUsesApproximateContentBoundsComparison(DependencyObject obj, bool value) {
    obj.SetValue(dp: UsesApproximateContentBoundsComparisonProperty, value: value);
  }

  public static readonly DependencyProperty UsesApproximateContentBoundsComparisonProperty = DependencyProperty.RegisterAttached(
    name: "UsesApproximateContentBoundsComparison",
    propertyType: typeof(bool),
    ownerType: typeof(Viewport2D),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: OnUsesApproximateContentBoundsComparisonChanged));

  private static void OnUsesApproximateContentBoundsComparisonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if(d is IPlotterElement element) {
      if(element.Plotter is PlotterBase plotter2d) {
        plotter2d.Viewport.UpdateVisible();
      }
    }
  }

  #endregion // end of UsesApproximateContentBoundsComparison

  #region UseDeferredPanning attached property

  public static bool GetUseDeferredPanning(DependencyObject obj) {
    return (bool)obj.GetValue(dp: UseDeferredPanningProperty);
  }

  public static void SetUseDeferredPanning(DependencyObject obj, bool value) {
    obj.SetValue(dp: UseDeferredPanningProperty, value: value);
  }

  public static readonly DependencyProperty UseDeferredPanningProperty = DependencyProperty.RegisterAttached(
    name: "UseDeferredPanning",
    propertyType: typeof(bool),
    ownerType: typeof(Viewport2D),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion // end of UseDeferredPanning attached property

  #endregion Attached Properties
}