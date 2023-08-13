// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Shapes;

namespace Crystal.Themes.Controls;

/// <summary>
///     Represents a container with two views; one view for the main content and another view that is typically used for
///     navigation commands.
/// </summary>
[TemplatePart(Name = "PaneClipRectangle", Type = typeof(RectangleGeometry))]
[TemplatePart(Name = "LightDismissLayer", Type = typeof(Rectangle))]
[TemplatePart(Name = "PART_ResizingThumb", Type = typeof(CrystalThumb))]
[TemplateVisualState(Name = "Closed", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "ClosedCompactLeft", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "ClosedCompactRight", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "OpenOverlayLeft", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "OpenOverlayRight", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "OpenInlineLeft", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "OpenInlineRight", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "OpenCompactOverlayLeft", GroupName = "DisplayModeStates")]
[TemplateVisualState(Name = "OpenCompactOverlayRight", GroupName = "DisplayModeStates")]
[ContentProperty(nameof(Content))]
[StyleTypedProperty(Property = nameof(ResizeThumbStyle), StyleTargetType = typeof(CrystalThumb))]
public class SplitView : Control
{
  private Rectangle? lightDismissLayer;
  private RectangleGeometry? paneClipRectangle;
  private CrystalThumb? resizingThumb;

  /// <summary>Identifies the <see cref="CompactPaneLength"/> dependency property.</summary>
  public static readonly DependencyProperty CompactPaneLengthProperty
    = DependencyProperty.Register(nameof(CompactPaneLength),
      typeof(double),
      typeof(SplitView),
      new PropertyMetadata(0d, OnCompactPaneLengthPropertyChangedCallback));

  private static void OnCompactPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue && e.NewValue is double && dependencyObject is SplitView splitView)
    {
      splitView.CoerceValue(OpenPaneLengthProperty);
      splitView.TemplateSettings?.Update();
      splitView.ChangeVisualState(true, true);
    }
  }

  /// <summary>
  ///     Gets or sets the width of the <see cref="SplitView"/> pane in its compact display mode.
  /// </summary>
  /// <returns>
  ///     The width of the pane in it's compact display mode. The default is 48 device-independent pixel (DIP) (defined
  ///     by the SplitViewCompactPaneThemeLength resource).
  /// </returns>
  public double CompactPaneLength
  {
    get => (double)GetValue(CompactPaneLengthProperty);
    set => SetValue(CompactPaneLengthProperty, value);
  }

  /// <summary>Identifies the <see cref="Content"/> dependency property.</summary>
  public static readonly DependencyProperty ContentProperty
    = DependencyProperty.Register(nameof(Content),
      typeof(UIElement),
      typeof(SplitView),
      new PropertyMetadata(null));

  /// <summary>
  ///     Gets or sets the contents of the main panel of a <see cref="SplitView"/>.
  /// </summary>
  /// <returns>The contents of the main panel of a <see cref="SplitView"/>. The default is null.</returns>
  public UIElement? Content
  {
    get => (UIElement?)GetValue(ContentProperty);
    set => SetValue(ContentProperty, value);
  }

  /// <summary>Identifies the <see cref="DisplayMode"/> dependency property.</summary>
  public static readonly DependencyProperty DisplayModeProperty
    = DependencyProperty.Register(nameof(DisplayMode),
      typeof(SplitViewDisplayMode),
      typeof(SplitView),
      new PropertyMetadata(SplitViewDisplayMode.Overlay, OnDisplayModePropertyChangedCallback));

  private static void OnDisplayModePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue && e.NewValue is SplitViewDisplayMode && dependencyObject is SplitView splitView)
    {
      splitView.CoerceValue(OpenPaneLengthProperty);
      splitView.ChangeVisualState(true, false);
    }
  }

  /// <summary>
  ///     Gets of sets a value that specifies how the pane and content areas of a <see cref="SplitView"/> are shown.
  /// </summary>
  /// <returns>
  ///     A value of the enumeration that specifies how the pane and content areas of a <see cref="SplitView"/> are
  ///     shown. The default is <see cref="SplitViewDisplayMode.Overlay"/>.
  /// </returns>
  public SplitViewDisplayMode DisplayMode
  {
    get => (SplitViewDisplayMode)GetValue(DisplayModeProperty);
    set => SetValue(DisplayModeProperty, value);
  }

  /// <summary>Identifies the <see cref="IsPaneOpen"/> dependency property.</summary>
  public static readonly DependencyProperty IsPaneOpenProperty
    = DependencyProperty.Register(nameof(IsPaneOpen),
      typeof(bool),
      typeof(SplitView),
      new PropertyMetadata(BooleanBoxes.FalseBox, OnIsPaneOpenChanged));

  private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is SplitView splitView && e.OldValue != e.NewValue && e.NewValue is bool isPaneOpen)
    {
      if (isPaneOpen)
      {
        splitView.ChangeVisualState(true, false); // Open pane
      }
      else
      {
        splitView.OnIsPaneOpenChanged(); // Close pane
      }
    }
  }

  /// <summary>
  ///     Gets or sets a value that specifies whether the <see cref="SplitView"/> pane is expanded to its full width.
  /// </summary>
  /// <returns>true if the pane is expanded to its full width; otherwise, false. The default is true.</returns>
  public bool IsPaneOpen
  {
    get => (bool)GetValue(IsPaneOpenProperty);
    set => SetValue(IsPaneOpenProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="OverlayBrush"/> dependency property.</summary>
  public static readonly DependencyProperty OverlayBrushProperty
    = DependencyProperty.Register(nameof(OverlayBrush),
      typeof(Brush),
      typeof(SplitView),
      new PropertyMetadata(Brushes.Transparent));

  /// <summary>
  /// Gets or sets a value that specifies the OverlayBrush 
  /// </summary>
  /// <returns>The current OverlayBrush</returns>
  public Brush OverlayBrush
  {
    get => (Brush)GetValue(OverlayBrushProperty);
    set => SetValue(OverlayBrushProperty, value);
  }

  /// <summary>Identifies the <see cref="OpenPaneLength"/> dependency property.</summary>
  public static readonly DependencyProperty OpenPaneLengthProperty
    = DependencyProperty.Register(nameof(OpenPaneLength),
      typeof(double),
      typeof(SplitView),
      new PropertyMetadata(0d, OnOpenPaneLengthPropertyChangedCallback, OnOpenPaneLengthCoerceValueCallback));

  [MustUseReturnValue]
  private static object? OnOpenPaneLengthCoerceValueCallback(DependencyObject dependencyObject, object? inputValue)
  {
    if (dependencyObject is SplitView splitView && splitView.ActualWidth > 0 && inputValue is double openPaneLength)
    {
      // Get the minimum needed width
      var minWidth = splitView.DisplayMode == SplitViewDisplayMode.CompactInline || splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay
        ? Math.Max(splitView.CompactPaneLength, splitView.MinimumOpenPaneLength)
        : Math.Max(0, splitView.MinimumOpenPaneLength);

      if (minWidth < 0)
      {
        minWidth = 0;
      }

      // Get the maximum allowed width
      var maxWidth = Math.Min(splitView.ActualWidth, splitView.MaximumOpenPaneLength);

      // Check if max < min
      if (maxWidth < minWidth)
      {
        minWidth = maxWidth;
      }

      // Check is OpenPaneLength is valid
      if (openPaneLength < minWidth)
      {
        return minWidth;
      }

      if (openPaneLength > maxWidth)
      {
        return maxWidth;
      }

      return openPaneLength;
    }

    return inputValue;
  }

  private static void OnOpenPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.NewValue != e.OldValue && dependencyObject is SplitView splitView)
    {
      splitView.TemplateSettings?.Update();
      splitView.ChangeVisualState(true, true);
    }
  }

  /// <summary>
  ///     Gets or sets the width of the <see cref="SplitView"/> pane when it's fully expanded.
  /// </summary>
  /// <returns>
  ///     The width of the <see cref="SplitView"/> pane when it's fully expanded. The default is 320 device-independent
  ///     pixel (DIP).
  /// </returns>
  public double OpenPaneLength
  {
    get => (double)GetValue(OpenPaneLengthProperty);
    set => SetValue(OpenPaneLengthProperty, value);
  }

  /// <summary>Identifies the <see cref="MinimumOpenPaneLength"/> dependency property.</summary>
  public static readonly DependencyProperty MinimumOpenPaneLengthProperty
    = DependencyProperty.Register(nameof(MinimumOpenPaneLength),
      typeof(double),
      typeof(SplitView),
      new PropertyMetadata(100d, MinimumOpenPaneLengthPropertyChangedCallback));

  private static void MinimumOpenPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue && e.NewValue is double && dependencyObject is SplitView splitView)
    {
      splitView.CoerceValue(OpenPaneLengthProperty);
      splitView.TemplateSettings?.Update();
      splitView.ChangeVisualState(true, true);
    }
  }

  /// <summary>
  ///     Gets or sets the minimum width of the <see cref="SplitView"/> pane when it's fully expanded.
  /// </summary>
  /// <returns>
  ///     The minimum width of the <see cref="SplitView"/> pane when it's fully expanded. The default is 320 device-independent
  ///     pixel (DIP).
  /// </returns>
  public double MinimumOpenPaneLength
  {
    get => (double)GetValue(MinimumOpenPaneLengthProperty);
    set => SetValue(MinimumOpenPaneLengthProperty, value);
  }

  /// <summary>Identifies the <see cref="MaximumOpenPaneLength"/> dependency property.</summary>
  public static readonly DependencyProperty MaximumOpenPaneLengthProperty
    = DependencyProperty.Register(nameof(MaximumOpenPaneLength),
      typeof(double),
      typeof(SplitView),
      new PropertyMetadata(500d, OnMaximumOpenPaneLengthPropertyChangedCallback));

  private static void OnMaximumOpenPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue && e.NewValue is double && dependencyObject is SplitView splitView)
    {
      splitView.CoerceValue(OpenPaneLengthProperty);
      splitView.TemplateSettings?.Update();
      splitView.ChangeVisualState(true, true);
    }
  }

  /// <summary>
  ///     Gets or sets the maximum width of the <see cref="SplitView"/> pane when it's fully expanded.
  /// </summary>
  /// <returns>
  ///     The maximum width of the <see cref="SplitView"/> pane when it's fully expanded. The default is 320 device-independent
  ///     pixel (DIP).
  /// </returns>
  public double MaximumOpenPaneLength
  {
    get => (double)GetValue(MaximumOpenPaneLengthProperty);
    set => SetValue(MaximumOpenPaneLengthProperty, value);
  }

  /// <summary>Identifies the <see cref="CanResizeOpenPane"/> dependency property.</summary>
  public static readonly DependencyProperty CanResizeOpenPaneProperty
    = DependencyProperty.Register(nameof(CanResizeOpenPane),
      typeof(bool),
      typeof(SplitView),
      new PropertyMetadata(BooleanBoxes.FalseBox, OnCanResizeOpenPanePropertyChangedCallback));

  private static void OnCanResizeOpenPanePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue && e.NewValue is bool && dependencyObject is SplitView splitView)
    {
      splitView.CoerceValue(OpenPaneLengthProperty);
    }
  }

  /// <summary>
  /// Gets or Sets if the open pane can be resized by the user. The default value is false.
  /// </summary>
  public bool CanResizeOpenPane
  {
    get => (bool)GetValue(CanResizeOpenPaneProperty);
    set => SetValue(CanResizeOpenPaneProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="ResizeThumbStyle"/> dependency property.</summary>
  public static readonly DependencyProperty ResizeThumbStyleProperty
    = DependencyProperty.Register(nameof(ResizeThumbStyle),
      typeof(Style),
      typeof(SplitView),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or Sets the <see cref="Style"/> for the resizing Thumb (type of <see cref="CrystalThumb"/>)
  /// </summary>
  public Style? ResizeThumbStyle
  {
    get => (Style?)GetValue(ResizeThumbStyleProperty);
    set => SetValue(ResizeThumbStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="Pane"/> dependency property.</summary>
  public static readonly DependencyProperty PaneProperty
    = DependencyProperty.Register(nameof(Pane),
      typeof(UIElement),
      typeof(SplitView),
      new PropertyMetadata(null, UpdateLogicalChild));

  /// <summary>
  ///     Gets or sets the contents of the pane of a <see cref="SplitView"/>.
  /// </summary>
  /// <returns>The contents of the pane of a <see cref="SplitView"/>. The default is null.</returns>
  public UIElement? Pane
  {
    get => (UIElement?)GetValue(PaneProperty);
    set => SetValue(PaneProperty, value);
  }

  /// <summary>Identifies the <see cref="PaneBackground"/> dependency property.</summary>
  public static readonly DependencyProperty PaneBackgroundProperty
    = DependencyProperty.Register(nameof(PaneBackground),
      typeof(Brush),
      typeof(SplitView),
      new PropertyMetadata(null));

  /// <summary>
  ///     Gets or sets the Brush to apply to the background of the <see cref="Pane"/> area of the control.
  /// </summary>
  /// <returns>The Brush to apply to the background of the <see cref="Pane"/> area of the control.</returns>
  public Brush? PaneBackground
  {
    get => (Brush?)GetValue(PaneBackgroundProperty);
    set => SetValue(PaneBackgroundProperty, value);
  }

  /// <summary>Identifies the <see cref="PaneForeground"/> dependency property.</summary>
  public static readonly DependencyProperty PaneForegroundProperty
    = DependencyProperty.Register(nameof(PaneForeground),
      typeof(Brush),
      typeof(SplitView),
      new PropertyMetadata(null));

  /// <summary>
  ///     Gets or sets the Brush to apply to the foreground of the <see cref="Pane"/> area of the control.
  /// </summary>
  /// <returns>The Brush to apply to the background of the <see cref="Pane"/> area of the control.</returns>
  public Brush? PaneForeground
  {
    get => (Brush?)GetValue(PaneForegroundProperty);
    set => SetValue(PaneForegroundProperty, value);
  }

  /// <summary>Identifies the <see cref="PanePlacement"/> dependency property.</summary>
  public static readonly DependencyProperty PanePlacementProperty
    = DependencyProperty.Register(nameof(PanePlacement),
      typeof(SplitViewPanePlacement),
      typeof(SplitView),
      new PropertyMetadata(SplitViewPanePlacement.Left, OnPanePlacementPropertyChangedCallback));

  private static void OnPanePlacementPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (e.OldValue != e.NewValue && e.NewValue is SplitViewPanePlacement && dependencyObject is SplitView splitView)
    {
      splitView.CoerceValue(OpenPaneLengthProperty);
      splitView.ChangeVisualState(true, false);
    }
  }

  /// <summary>
  ///     Gets or sets a value that specifies whether the pane is shown on the right or left side of the
  ///     <see cref="SplitView"/>.
  /// </summary>
  /// <returns>
  ///     A value of the enumeration that specifies whether the pane is shown on the right or left side of the
  ///     <see cref="SplitView"/>. The default is <see cref="SplitViewPanePlacement.Left"/>.
  /// </returns>
  public SplitViewPanePlacement PanePlacement
  {
    get => (SplitViewPanePlacement)GetValue(PanePlacementProperty);
    set => SetValue(PanePlacementProperty, value);
  }

  /// <summary>Identifies the <see cref="TemplateSettings"/> dependency property.</summary>
  public static readonly DependencyProperty TemplateSettingsProperty
    = DependencyProperty.Register(nameof(TemplateSettings),
      typeof(SplitViewTemplateSettings),
      typeof(SplitView),
      new PropertyMetadata(null));

  /// <summary>
  ///     Gets an object that provides calculated values that can be referenced as TemplateBinding sources when defining
  ///     templates for a <see cref="SplitView"/> control.
  /// </summary>
  /// <returns>An object that provides calculated values for templates.</returns>
  public SplitViewTemplateSettings? TemplateSettings
  {
    get => (SplitViewTemplateSettings?)GetValue(TemplateSettingsProperty);
    private set => SetValue(TemplateSettingsProperty, value);
  }

  /// <summary>
  ///     Initializes a new instance of the <see cref="SplitView"/> class.
  /// </summary>
  public SplitView()
  {
    DefaultStyleKey = typeof(SplitView);
    TemplateSettings = new SplitViewTemplateSettings(this);
    DataContextChanged += SplitViewDataContextChanged;
  }

  private void SplitViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
  {
    // Crystal add this pane to the SplitView with AddLogicalChild method.
    // This has the side effect that the DataContext doesn't update, so do this now here.
    if (Pane is FrameworkElement elementPane)
    {
      elementPane.DataContext = DataContext;
    }
  }

  /// <summary>
  ///     Occurs when the <see cref="SplitView"/> pane is closed.
  /// </summary>
  public event EventHandler? PaneClosed;

  /// <summary>
  ///     Occurs when the <see cref="SplitView"/> pane is closing.
  /// </summary>
  public event EventHandler<SplitViewPaneClosingEventArgs>? PaneClosing;

  /// <inheritdoc/>
  public override void OnApplyTemplate()
  {
    if (lightDismissLayer != null)
    {
      lightDismissLayer.MouseDown -= OnLightDismiss;
    }

    if (resizingThumb != null)
    {
      resizingThumb.DragDelta -= ResizingThumb_DragDelta;
    }

    base.OnApplyTemplate();

    paneClipRectangle = GetTemplateChild("PaneClipRectangle") as RectangleGeometry;

    lightDismissLayer = GetTemplateChild("LightDismissLayer") as Rectangle;
    if (lightDismissLayer != null)
    {
      lightDismissLayer.MouseDown += OnLightDismiss;
    }

    resizingThumb = GetTemplateChild("PART_ResizingThumb") as CrystalThumb;
    if (resizingThumb != null)
    {
      resizingThumb.DragDelta += ResizingThumb_DragDelta;
    }

    this.ExecuteWhenLoaded(() =>
    {
      TemplateSettings?.Update();
      ChangeVisualState(false, false);
    });
  }

  private void ResizingThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
  {
    SetCurrentValue(OpenPaneLengthProperty, PanePlacement == SplitViewPanePlacement.Left ? OpenPaneLength + e.HorizontalChange : OpenPaneLength - e.HorizontalChange);
  }

  private static void UpdateLogicalChild(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
  {
    if (dependencyObject is not SplitView splitView)
    {
      return;
    }

    if (e.OldValue is FrameworkElement oldChild)
    {
      splitView.RemoveLogicalChild(oldChild);
    }

    if (e.NewValue is FrameworkElement newChild)
    {
      splitView.AddLogicalChild(newChild);
      newChild.DataContext = splitView.DataContext;
    }
  }

  /// <inheritdoc/>
  protected override IEnumerator LogicalChildren
  {
    get
    {
      // cheat, make a list with all logical content and return the enumerator
      ArrayList children = new ArrayList();
      if (Pane != null)
      {
        children.Add(Pane);
      }

      if (Content != null)
      {
        children.Add(Content);
      }

      return children.GetEnumerator();
    }
  }

  /// <inheritdoc/>
  protected override void OnRenderSizeChanged(SizeChangedInfo info)
  {
    base.OnRenderSizeChanged(info);

    if (IsPaneOpen)
    {
      CoerceValue(OpenPaneLengthProperty);
    }

    if (paneClipRectangle != null)
    {
      paneClipRectangle.Rect = new Rect(0, 0, OpenPaneLength, ActualHeight);
    }
  }

  protected virtual void ChangeVisualState(bool animated, bool reset)
  {
    if (paneClipRectangle != null)
    {
      paneClipRectangle.Rect = new Rect(0, 0, OpenPaneLength, ActualHeight); // We could also use ActualHeight and subscribe to the SizeChanged property
    }

    var state = string.Empty;
    if (IsPaneOpen)
    {
      state += "Open";
      switch (DisplayMode)
      {
        case SplitViewDisplayMode.CompactInline:
          state += "Inline";
          break;
        default:
          state += DisplayMode.ToString();
          break;
      }

      state += PanePlacement.ToString();
    }
    else
    {
      state += "Closed";
      if (DisplayMode == SplitViewDisplayMode.CompactInline
          || DisplayMode == SplitViewDisplayMode.CompactOverlay)
      {
        state += "Compact";
        state += PanePlacement.ToString();
      }
    }

    if (reset)
    {
      VisualStateManager.GoToState(this, "None", animated);
    }

    VisualStateManager.GoToState(this, state, animated);
  }

  protected void OnIsPaneOpenChanged()
  {
    var cancel = false;

    if (PaneClosing != null)
    {
      var args = new SplitViewPaneClosingEventArgs();
      foreach (var paneClosingDelegates in PaneClosing.GetInvocationList())
      {
        var eventHandler = paneClosingDelegates as EventHandler<SplitViewPaneClosingEventArgs>;
        if (eventHandler is null)
        {
          continue;
        }

        eventHandler(this, args);
        if (args.Cancel)
        {
          cancel = true;
          break;
        }
      }
    }

    if (!cancel)
    {
      ChangeVisualState(true, false);
      PaneClosed?.Invoke(this, EventArgs.Empty);
    }
    else
    {
      SetCurrentValue(IsPaneOpenProperty, BooleanBoxes.FalseBox);
    }
  }

  private void OnLightDismiss(object sender, MouseButtonEventArgs e)
  {
    SetCurrentValue(IsPaneOpenProperty, BooleanBoxes.FalseBox);
  }
}