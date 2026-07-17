using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Common.NotifyingPanels;
using Crystal.Plot2D.Common.UndoSystem;
using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using static Crystal.Plot2D.Constants.Constants;

namespace Crystal.Plot2D.Common;

[ContentProperty(name: "Children")]
[TemplatePart(Name = "PART_HeaderPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_FooterPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_BottomPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_LeftPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_RightPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_TopPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_MainCanvas", Type = typeof(Canvas))]
[TemplatePart(Name = "PART_CentralGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_MainGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_ContentsGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_ParallelCanvas", Type = typeof(Canvas))]
public abstract class PlotterBase : ContentControl {
  protected PlotterLoadMode LoadMode { get; }

  #region Fields

  private readonly Stack<IPlotterElement> _addingElements = new();
  private readonly Stack<IPlotterElement> _removingElements = new();
  private readonly Dictionary<IPlotterElement, List<UIElement>> _addedVisualElements = new();
  private readonly List<Action> _waitingForExecute = new();
  private const string StyleKey = "DefaultPlotterStyle";
  private const string TemplateKey = "DefaultPlotterTemplate";

  #endregion Fields

  #region Properties

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel BottomPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel CentralGrid { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public PlotterChildrenCollection Children { [DebuggerStepThrough] get; }

  protected IPlotterElement CurrentChild { get; private set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel FooterPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel LeftPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel RightPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel TopPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel MainCanvas { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel MainGrid { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel ParallelCanvas { get; protected set; }

  protected ResourceDictionary GenericResources { get; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel HeaderPanel { get; protected set; }

  #endregion Properties

  #region Constructors

  protected PlotterBase() : this(loadMode: PlotterLoadMode.Normal) {
    Children.CollectionChanged += (s, e) => viewportInstance.UpdateIterationCount = 0;
    InitViewport();
  }

  protected PlotterBase(PlotterLoadMode loadMode) {
    LoadMode = loadMode;
    SetPlotter(obj: this, value: this);
    if(loadMode == PlotterLoadMode.Normal) {
      UpdateUIParts();
    }
    Children = new PlotterChildrenCollection(plotter: this);
    Children.CollectionChanged += OnChildrenCollectionChanged;
    Loaded += Plotter_Loaded;
    Unloaded += Plotter_Unloaded;
    if(LoadMode != PlotterLoadMode.Empty) {
      InitViewport();
    }
    var uri_ = new Uri(uriString: ThemeUri, uriKind: UriKind.Relative);
    GenericResources = (ResourceDictionary)Application.LoadComponent(resourceLocator: uri_);
    ContextMenu = null;
  }

  #endregion Constructors

  #region Loaded & Unloaded

  private void Plotter_Loaded(object sender, RoutedEventArgs e) {
    ExecuteWaitingChildrenAdditions();
    OnLoaded();
  }

  protected void OnLoaded() => Focus();

  private void Plotter_Unloaded(object sender, RoutedEventArgs e) => OnUnloaded();

  protected void OnUnloaded() { }

  #endregion Loaded & Unloaded

  protected override AutomationPeer OnCreateAutomationPeer() => new PlotterAutomationPeer(owner: this);

  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public override bool ShouldSerializeContent() => false;

  protected override bool ShouldSerializeProperty(DependencyProperty dp) {
    if(dp == ContextMenuProperty) {
      return false;
    }
    if(dp == TemplateProperty || dp == ContentProperty) {
      return false;
    }
    return base.ShouldSerializeProperty(dp: dp);
  }

  private void UpdateUIParts() {
    Resources = new ResourceDictionary { Source = new Uri(uriString: PlotterResourceUri, uriKind: UriKind.Relative) };
    Style = (Style)FindResource(StyleKey) ?? throw new ResourceReferenceKeyNotFoundException();
    Template = (ControlTemplate)FindResource(TemplateKey) ?? throw new ResourceReferenceKeyNotFoundException();
    ApplyTemplate();
  }

  public void PerformLoad() {
    _isLoadedIntensively = true;
    ApplyTemplate();
    Plotter_Loaded(sender: null, e: null);
  }

  private bool _isLoadedIntensively;
  protected bool IsLoadedInternal => _isLoadedIntensively || IsLoaded;

  protected internal void ExecuteWaitingChildrenAdditions() {
    foreach(var action_ in _waitingForExecute) {
      action_();
    }
    _waitingForExecute.Clear();
  }

  private Grid _contentsGrid;
  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    _addedVisualElements.Clear();
    foreach(var item_ in GetAllPanels()) {
      if(item_ is not INotifyingPanel panel_) continue;
      panel_.ChildrenCreated -= NotifyingItem_ChildrenCreated;
      if(panel_.NotifyingChildren != null) {
        panel_.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
      }
    }

    var headerPanel_ = GetPart<StackPanel>(name: "PART_HeaderPanel");
    MigrateChildren(previousParent: HeaderPanel, currentParent: headerPanel_);
    HeaderPanel = headerPanel_;

    var footerPanel_ = GetPart<StackPanel>(name: "PART_FooterPanel");
    MigrateChildren(previousParent: FooterPanel, currentParent: footerPanel_);
    FooterPanel = footerPanel_;

    var leftPanel_ = GetPart<StackPanel>(name: "PART_LeftPanel");
    MigrateChildren(previousParent: LeftPanel, currentParent: leftPanel_);
    LeftPanel = leftPanel_;

    var bottomPanel_ = GetPart<StackPanel>(name: "PART_BottomPanel");
    MigrateChildren(previousParent: BottomPanel, currentParent: bottomPanel_);
    BottomPanel = bottomPanel_;

    var rightPanel_ = GetPart<StackPanel>(name: "PART_RightPanel");
    MigrateChildren(previousParent: RightPanel, currentParent: rightPanel_);
    RightPanel = rightPanel_;

    var topPanel_ = GetPart<StackPanel>(name: "PART_TopPanel");
    MigrateChildren(previousParent: TopPanel, currentParent: topPanel_);
    TopPanel = topPanel_;

    var mainCanvas_ = GetPart<Canvas>(name: "PART_MainCanvas");
    MigrateChildren(previousParent: MainCanvas, currentParent: mainCanvas_);
    MainCanvas = mainCanvas_;

    var centralGrid_ = GetPart<Grid>(name: "PART_CentralGrid");
    MigrateChildren(previousParent: CentralGrid, currentParent: centralGrid_);
    CentralGrid = centralGrid_;

    var mainGrid_ = GetPart<Grid>(name: "PART_MainGrid");
    MigrateChildren(previousParent: MainGrid, currentParent: mainGrid_);
    MainGrid = mainGrid_;

    var parallelCanvas_ = GetPart<Canvas>(name: "PART_ParallelCanvas");
    MigrateChildren(previousParent: ParallelCanvas, currentParent: parallelCanvas_);
    ParallelCanvas = parallelCanvas_;

    var contentsGrid_ = GetPart<Grid>(name: "PART_ContentsGrid");
    MigrateChildren(previousParent: contentsGrid_, currentParent: contentsGrid_);
    _contentsGrid = contentsGrid_;

    Content = contentsGrid_;
    AddLogicalChild(child: contentsGrid_);

    foreach(var notifyingItem_ in GetAllPanels()) {
      if(notifyingItem_ is not INotifyingPanel panel_) continue;
      if(panel_.NotifyingChildren == null) {
        panel_.ChildrenCreated += NotifyingItem_ChildrenCreated;
      }
      else {
        panel_.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
      }
    }
  }

  private static void MigrateChildren(Panel previousParent, Panel currentParent) {
    if(previousParent != null && currentParent != null) {
      var children = new UIElement[previousParent.Children.Count];
      previousParent.Children.CopyTo(array: children, index: 0);
      previousParent.Children.Clear();

      foreach(var child in children) {
        if(!currentParent.Children.Contains(element: child)) {
          currentParent.Children.Add(element: child);
        }
      }
    }
    else if(previousParent != null) {
      previousParent.Children.Clear();
    }
  }

  private void NotifyingItem_ChildrenCreated(object sender, EventArgs e) {
    var panel_ = (INotifyingPanel)sender;
    SubscribePanelEvents(panel: panel_);
  }

  private void SubscribePanelEvents(INotifyingPanel panel) {
    panel.ChildrenCreated -= NotifyingItem_ChildrenCreated;
    panel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
    panel.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
  }

  private void OnVisualCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    if(e.NewItems != null) {
      foreach(var item_ in e.NewItems) {
        if(item_ is INotifyingPanel notifyingPanel_) {
          if(notifyingPanel_.NotifyingChildren != null) {
            notifyingPanel_.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
            notifyingPanel_.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
          }
          else {
            notifyingPanel_.ChildrenCreated += NotifyingItem_ChildrenCreated;
          }
        }
        OnVisualChildAdded(target: (UIElement)item_);
      }
    }

    if(e.OldItems == null) return;
    foreach(var item_ in e.OldItems) {
      if(item_ is INotifyingPanel notifyingPanel_) {
        notifyingPanel_.ChildrenCreated -= NotifyingItem_ChildrenCreated;
        if(notifyingPanel_.NotifyingChildren != null) {
          notifyingPanel_.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
        }
      }

      OnVisualChildRemoved(target: (UIElement)item_);
    }
  }

  public VisualBindingCollection VisualBindings { get; } = new();

  protected void OnVisualChildAdded(UIElement target) {
    if(_addingElements.Count > 0) {
      var element = _addingElements.Peek();

      var dict = VisualBindings.Cache;
      var proxy = dict[key: element];

      List<UIElement> visualElements;
      if(!_addedVisualElements.ContainsKey(key: element)) {
        visualElements = new List<UIElement>();
        _addedVisualElements.Add(key: element, value: visualElements);
      }
      else {
        visualElements = _addedVisualElements[key: element];
      }

      visualElements.Add(item: target);

      SetBindings(proxy: proxy, target: target);
    }
  }

  private static void SetBindings(UIElement proxy, UIElement target) {
    if(proxy == target) return;
    foreach(var property_ in GetPropertiesToSetBindingOn()) {
      BindingOperations.SetBinding(target: target, dp: property_,
        binding: new Binding { Path = new PropertyPath(path: property_.Name), Source = proxy, Mode = BindingMode.TwoWay });
    }
  }

  private static void RemoveBindings(UIElement proxy, UIElement target) {
    if(proxy == target) return;
    foreach(var property_ in GetPropertiesToSetBindingOn()) {
      BindingOperations.ClearBinding(target: target, dp: property_);
    }
  }

  private static IEnumerable<DependencyProperty> GetPropertiesToSetBindingOn() {
    yield return OpacityProperty;
    yield return VisibilityProperty;
    yield return IsHitTestVisibleProperty;
    //yield return FrameworkElement.DataContextProperty;
  }

  protected void OnVisualChildRemoved(UIElement target) {
    if(_removingElements.Count <= 0) return;
    var element = _removingElements.Peek();
    var dict = VisualBindings.Cache;
    var proxy = dict[key: element];

    if(_addedVisualElements.ContainsKey(key: element)) {
      var list = _addedVisualElements[key: element];
      list.Remove(item: target);

      if(list.Count == 0) {
        dict.Remove(key: element);
      }

      _addedVisualElements.Remove(key: element);
    }
    RemoveBindings(proxy: proxy, target: target);
  }

  internal IEnumerable<Panel> GetAllPanels() {
    yield return HeaderPanel;
    yield return FooterPanel;

    yield return LeftPanel;
    yield return BottomPanel;
    yield return RightPanel;
    yield return TopPanel;

    yield return MainCanvas;
    yield return CentralGrid;
    yield return MainGrid;
    yield return ParallelCanvas;
    yield return _contentsGrid;
  }

  private T GetPart<T>(string name) {
    return (T)Template.FindName(name: name, templatedParent: this);
  }

  private bool _executedWaitingChildrenAdding;
  private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
    if(IsLoadedInternal && !_executedWaitingChildrenAdding) {
      _executedWaitingChildrenAdding = true;
      ExecuteWaitingChildrenAdditions();
    }

    AddChildren(e);
    RemoveChildren(e);
  }

  private void AddChildren(NotifyCollectionChangedEventArgs e) {
    if(e.NewItems == null) return;
    foreach(IPlotterElement item_ in e.NewItems) {
      if(IsLoadedInternal) {
        OnChildAdded(child: item_);
      }
      else {
        _waitingForExecute.Add(item: () => OnChildAdded(child: item_));
      }
    }
  }

  private void RemoveChildren(NotifyCollectionChangedEventArgs e) {
    if(e.OldItems == null) return;
    foreach(IPlotterElement item_ in e.OldItems) {
      if(IsLoadedInternal) {
        OnChildRemoved(child: item_);
      }
      else {
        _waitingForExecute.Add(item: () => OnChildRemoved(child: item_));
      }
    }
  }

  internal bool PerformChildChecks { get; } = true;

  private UIElement CreateVisualProxy(IPlotterElement child) {
    if(VisualBindings.Cache.ContainsKey(key: child)) {
      throw new InvalidOperationException(message: Strings.Exceptions.VisualBindingsWrongState);
    }
    if(child is not UIElement result) {
      result = new UIElement();
    }
    return result;
  }

  protected void OnChildAdded(IPlotterElement child) {
    if(child == null) return;
    _addingElements.Push(item: child);
    CurrentChild = child;

    try {
      var visualProxy = CreateVisualProxy(child: child);
      VisualBindings.Cache.Add(key: child, value: visualProxy);

      if(PerformChildChecks && child.Plotter != null) {
        throw new InvalidOperationException(message: Strings.Exceptions.PlotterElementAddedToAnotherPlotter);
      }

      if(child is FrameworkElement styleableElement) {
        var key = styleableElement.GetType();
        if(GenericResources.Contains(key: key)) {
          var elementStyle = (Style)GenericResources[key: key];
          styleableElement.Style = elementStyle;
        }
      }

      if(PerformChildChecks) {
        child.OnPlotterAttached(plotter: this);
        if(child.Plotter != this) {
          throw new InvalidOperationException(message: Strings.Exceptions.InvalidParentPlotterValue);
        }
      }

      if(child is DependencyObject dependencyObject) {
        SetPlotter(obj: dependencyObject, value: this);
      }
    }
    finally {
      _addingElements.Pop();
      CurrentChild = null;
    }
  }

  protected void OnChildRemoved(IPlotterElement child) {
    if(child == null) return;
    CurrentChild = child;
    _removingElements.Push(item: child);

    try {
      // todo probably here child.Plotter can be null.
      if(PerformChildChecks && child.Plotter != this && child.Plotter != null) {
        throw new InvalidOperationException(message: Strings.Exceptions.InvalidParentPlotterValueRemoving);
      }

      if(PerformChildChecks) {
        if(child.Plotter != null) {
          child.OnPlotterDetaching(plotter: this);
        }

        if(child.Plotter != null) {
          throw new InvalidOperationException(message: Strings.Exceptions.ParentPlotterNotNull);
        }
      }

      if(child is DependencyObject dependencyObject_) {
        SetPlotter(obj: dependencyObject_, value: null);
      }

      VisualBindings.Cache.Remove(key: child);

      if(_addedVisualElements.ContainsKey(key: child) && _addedVisualElements[key: child].Count > 0) {
        throw new InvalidOperationException(message: string.Format(format: Strings.Exceptions.PlotterElementDidnotCleanedAfterItself, arg0: child));
      }
    }
    finally {
      CurrentChild = null;
      _removingElements.Pop();
    }
  }

  #region Screenshots & copy to clipboard

  public BitmapSource CreateScreenShot() {
    Rect renderBounds_ = new(size: RenderSize);
    var p1_ = renderBounds_.TopLeft;
    var p2_ = renderBounds_.BottomRight;
    var rect_ = new Rect(point1: p1_, point2: p2_).ToInt32Rect();

    return ScreenshotHelper.CreateScreenshot(uiElement: this, screenshotSource: rect_);
  }

  public void SaveScreenShot(string filePath) {
    ScreenshotHelper.SaveBitmapToFile(bitmap: CreateScreenShot(), filePath: filePath);
  }

  /// <summary>
  /// Saves screenshot to stream.
  /// </summary>
  /// <param name="stream">The stream.</param>
  /// <param name="fileExtension">The file type extension.</param>
  public void SaveScreenShotToStream(Stream stream, string fileExtension) {
    ScreenshotHelper.SaveBitmapToStream(bitmap: CreateScreenShot(), stream: stream, fileExtension: fileExtension);
  }

  /// <summary>Copies the screenshot to clipboard.</summary>
  public void CopyScreenshotToClipboard() {
    Clipboard.Clear();
    Clipboard.SetImage(image: CreateScreenShot());
  }

  #endregion

  #region IsDefaultElement attached property

  protected void SetAllChildrenAsDefault() {
    foreach(var child in Children.OfType<DependencyObject>()) {
      child.SetValue(dp: IsDefaultElementProperty, value: true);
    }
  }

  /// <summary>Gets a value whether specified graphics object is default to this plotter or not</summary>
  /// <param name="obj">Graphics object to check</param>
  /// <returns>True if it is default or false otherwise</returns>
  public static bool GetIsDefaultElement(DependencyObject obj) {
    return (bool)obj.GetValue(dp: IsDefaultElementProperty);
  }

  public static void SetIsDefaultElement(DependencyObject obj, bool value) {
    obj.SetValue(dp: IsDefaultElementProperty, value: value);
  }

  public static readonly DependencyProperty IsDefaultElementProperty = DependencyProperty.RegisterAttached(
    name: "IsDefaultElement",
    propertyType: typeof(bool),
    ownerType: typeof(PlotterBase),
    defaultMetadata: new UIPropertyMetadata(defaultValue: false));

  /// <summary>Removes all user graphs from given UIElementCollection, 
  /// leaving only default graphs</summary>
  protected static void RemoveUserElements(IList<IPlotterElement> elements) {
    var index = 0;

    while(index < elements.Count) {
      if(elements[index: index] is DependencyObject d && !GetIsDefaultElement(obj: d)) {
        elements.RemoveAt(index: index);
      }
      else {
        index++;
      }
    }
  }

  public void RemoveUserElements() {
    RemoveUserElements(elements: Children);
  }

  #endregion

  #region IsDefaultAxis

  public static bool GetIsDefaultAxis(DependencyObject obj) {
    return (bool)obj.GetValue(dp: IsDefaultAxisProperty);
  }

  public static void SetIsDefaultAxis(DependencyObject obj, bool value) {
    obj.SetValue(dp: IsDefaultAxisProperty, value: value);
  }

  public static readonly DependencyProperty IsDefaultAxisProperty = DependencyProperty.RegisterAttached(
    name: "IsDefaultAxis",
    propertyType: typeof(bool),
    ownerType: typeof(PlotterBase),
    defaultMetadata: new UIPropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsDefaultAxisChanged));

  private static void OnIsDefaultAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    if(d is IPlotterElement plotterElement) {
      var parentPlotter = plotterElement.Plotter;
      parentPlotter?.OnIsDefaultAxisChangedCore(d: d, e: e);
    }
  }

  protected virtual void OnIsDefaultAxisChangedCore(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

  #endregion

  #region Undo

  public UndoProvider UndoProvider { get; } = new();

  /// <summary>
  /// Gets or sets the panel, which contains viewport.
  /// </summary>
  /// <value>
  /// The viewport panel.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel ViewportPanel { get; protected set; }

  /// <summary>
  ///   Gets the viewport.
  /// </summary>
  /// <value>
  ///   The viewport.
  /// </value>
  [NotNull]
  public Viewport2D Viewport {
    get {
      var useDeferredPanning = false;
      if(CurrentChild is DependencyObject dependencyChild) {
        useDeferredPanning = Viewport2D.GetUseDeferredPanning(obj: dependencyChild);
      }

      if(useDeferredPanning) {
        return deferredPanningProxy ??= new Viewport2dDeferredPanningProxy(viewport: viewportInstance);
      }

      return viewportInstance;
    }
    protected set => viewportInstance = value;
  }

  /// <summary>
  /// Gets or sets the CoordinateTransform.
  /// </summary>
  /// <value>
  /// The Transform.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public CoordinateTransform Transform {
    get => viewportInstance.Transform;
    set => viewportInstance.Transform = value;
  }

  /// <summary>
  ///   Gets or sets the visible area rectangle.
  /// </summary>
  /// <value>
  ///   The visible.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public DataRect Visible {
    get => viewportInstance.Visible;
    set => viewportInstance.Visible = value;
  }

  #endregion

  #region Plotter attached property

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public static PlotterBase GetPlotter(DependencyObject obj) {
    return (PlotterBase)obj.GetValue(dp: PlotterProperty);
  }

  public static void SetPlotter(DependencyObject obj, PlotterBase value) {
    obj.SetValue(dp: PlotterProperty, value: value);
  }

  public static readonly DependencyProperty PlotterProperty = DependencyProperty.RegisterAttached(
    name: nameof(Plotter),
    propertyType: typeof(PlotterBase),
    ownerType: typeof(PlotterBase),
    defaultMetadata: new FrameworkPropertyMetadata(
      defaultValue: null,
      flags: FrameworkPropertyMetadataOptions.Inherits,
      propertyChangedCallback: OnPlotterChanged));

  private static void OnPlotterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var previousPlotter_ = (PlotterBase)e.OldValue;
    var currentPlotter_ = (PlotterBase)e.NewValue;

    // raise Plotter[*] events, where * is Attached, Detaching, Changed.
    if(d is not FrameworkElement element_) return;
    PlotterChangedEventArgs args_ = new(prevPlotter: previousPlotter_, currPlotter: currentPlotter_, routedEvent: PlotterDetachingEvent);

    if(currentPlotter_ == null && previousPlotter_ != null) {
      RaisePlotterEvent(element: element_, args: args_);
    }
    else if(currentPlotter_ != null) {
      args_.RoutedEvent = PlotterAttachedEvent;
      RaisePlotterEvent(element: element_, args: args_);
    }

    args_.RoutedEvent = PlotterChangedEvent;
    RaisePlotterEvent(element: element_, args: args_);
  }

  private static void RaisePlotterEvent(FrameworkElement element, PlotterChangedEventArgs args) {
    element.RaiseEvent(e: args);
    PlotterEvents.Notify(target: element, args: args);
  }

  #endregion

  #region Plotter routed events

  public static readonly RoutedEvent PlotterAttachedEvent = EventManager.RegisterRoutedEvent(
    name: "PlotterAttached",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(PlotterChangedEventHandler),
    ownerType: typeof(PlotterBase));

  public static readonly RoutedEvent PlotterDetachingEvent = EventManager.RegisterRoutedEvent(
    name: "PlotterDetaching",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(PlotterChangedEventHandler),
    ownerType: typeof(PlotterBase));

  public static readonly RoutedEvent PlotterChangedEvent = EventManager.RegisterRoutedEvent(
    name: "PlotterChanged",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(PlotterChangedEventHandler),
    ownerType: typeof(PlotterBase));

  protected Viewport2D viewportInstance;
  private Viewport2dDeferredPanningProxy deferredPanningProxy;

  #endregion

  protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
    // This is part of endless axis resize loop workaround
    if(viewportInstance != null) {
      viewportInstance.UpdateIterationCount = 0;
      if(!viewportInstance.EnforceRestrictions) {
        Debug.WriteLine(message: "Plotter: enabling viewport constraints");
        viewportInstance.EnforceRestrictions = true;
      }
    }
    base.OnRenderSizeChanged(sizeInfo: sizeInfo);
  }

  public void FitToView() => viewportInstance.FitToView();

  protected void InitViewport() {
    ViewportPanel = new Canvas();
    Grid.SetColumn(element: ViewportPanel, value: 1);
    Grid.SetRow(element: ViewportPanel, value: 1);

    viewportInstance = new Viewport2D(host: ViewportPanel, plotter: this);
    if(LoadMode != PlotterLoadMode.Empty) {
      MainGrid.Children.Add(element: ViewportPanel);
    }
  }
}

public delegate void PlotterChangedEventHandler(object sender, PlotterChangedEventArgs e);
