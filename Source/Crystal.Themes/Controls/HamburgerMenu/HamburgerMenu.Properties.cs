using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  [StyleTypedProperty(Property = nameof(ResizeThumbStyle), StyleTargetType = typeof(CrystalThumb))]
  [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
  [StyleTypedProperty(Property = nameof(HeaderItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
  [StyleTypedProperty(Property = nameof(SeparatorItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
  [StyleTypedProperty(Property = nameof(ItemFocusVisualStyle), StyleTargetType = typeof(Control))]
  public partial class HamburgerMenu
  {
    private ControlTemplate? _defaultItemFocusVisualTemplate = null;

    public static readonly DependencyProperty OpenPaneLengthProperty = DependencyProperty.Register(
      nameof(OpenPaneLength),
      typeof(double),
      typeof(HamburgerMenu),
      new PropertyMetadata(240.0, OpenPaneLengthPropertyChangedCallback, OnOpenPaneLengthCoerceValueCallback));

    [MustUseReturnValue]
    private static object? OnOpenPaneLengthCoerceValueCallback(DependencyObject dependencyObject, object? inputValue)
    {
      if (dependencyObject is HamburgerMenu hamburgerMenu && hamburgerMenu.ActualWidth > 0 && inputValue is double openPaneLength)
      {
        // Get the minimum needed width
        var minWidth = hamburgerMenu.DisplayMode == SplitViewDisplayMode.CompactInline || hamburgerMenu.DisplayMode == SplitViewDisplayMode.CompactOverlay
            ? Math.Max(hamburgerMenu.CompactPaneLength, hamburgerMenu.MinimumOpenPaneLength)
            : Math.Max(0, hamburgerMenu.MinimumOpenPaneLength);

        if (minWidth < 0)
        {
          minWidth = 0;
        }

        // Get the maximum allowed width
        var maxWidth = Math.Min(hamburgerMenu.ActualWidth, hamburgerMenu.MaximumOpenPaneLength);

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

    private static void OpenPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
      if (args.NewValue != args.OldValue && dependencyObject is HamburgerMenu hamburgerMenu)
      {
        hamburgerMenu.ChangeItemFocusVisualStyle();
      }
    }

    public double OpenPaneLength
    {
      get => (double)GetValue(OpenPaneLengthProperty);
      set => SetValue(OpenPaneLengthProperty, value);
    }

    public static readonly DependencyProperty CompactPaneLengthProperty = DependencyProperty.Register(
      nameof(CompactPaneLength),
      typeof(double),
      typeof(HamburgerMenu),
      new PropertyMetadata(48.0, OnCompactPaneLengthPropertyChangedCallback));

    private static void OnCompactPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && e.NewValue is double && dependencyObject is HamburgerMenu hamburgerMenu)
      {
        hamburgerMenu.CoerceValue(OpenPaneLengthProperty);
        hamburgerMenu.ChangeItemFocusVisualStyle();
      }
    }

    public double CompactPaneLength
    {
      get => (double)GetValue(CompactPaneLengthProperty);
      set => SetValue(CompactPaneLengthProperty, value);
    }

    public static readonly DependencyProperty MinimumOpenPaneLengthProperty = DependencyProperty.Register(
      nameof(MinimumOpenPaneLength),
      typeof(double),
      typeof(HamburgerMenu),
      new PropertyMetadata(100d, MinimumOpenPaneLengthPropertyChangedCallback));

    private static void MinimumOpenPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && e.NewValue is double && dependencyObject is HamburgerMenu hamburgerMenu)
      {
        hamburgerMenu.CoerceValue(OpenPaneLengthProperty);
        hamburgerMenu.ChangeItemFocusVisualStyle();
      }
    }

    public double MinimumOpenPaneLength
    {
      get => (double)GetValue(MinimumOpenPaneLengthProperty);
      set => SetValue(MinimumOpenPaneLengthProperty, value);
    }

    public static readonly DependencyProperty MaximumOpenPaneLengthProperty = DependencyProperty.Register(
      nameof(MaximumOpenPaneLength),
      typeof(double),
      typeof(HamburgerMenu),
      new PropertyMetadata(500d, OnMaximumOpenPaneLengthPropertyChangedCallback));

    private static void OnMaximumOpenPaneLengthPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && e.NewValue is double && dependencyObject is HamburgerMenu hamburgerMenu)
      {
        hamburgerMenu.CoerceValue(OpenPaneLengthProperty);
        hamburgerMenu.ChangeItemFocusVisualStyle();
      }
    }

    public double MaximumOpenPaneLength
    {
      get => (double)GetValue(MaximumOpenPaneLengthProperty);
      set => SetValue(MaximumOpenPaneLengthProperty, value);
    }

    public static readonly DependencyProperty CanResizeOpenPaneProperty = DependencyProperty.Register(
      nameof(CanResizeOpenPane),
      typeof(bool),
      typeof(HamburgerMenu),
      new PropertyMetadata(BooleanBoxes.FalseBox, OnCanResizeOpenPanePropertyChangedCallback));

    private static void OnCanResizeOpenPanePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && e.NewValue is bool && dependencyObject is HamburgerMenu hamburgerMenu)
      {
        hamburgerMenu.CoerceValue(OpenPaneLengthProperty);
      }
    }

    public bool CanResizeOpenPane
    {
      get => (bool)GetValue(CanResizeOpenPaneProperty);
      set => SetValue(CanResizeOpenPaneProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty ResizeThumbStyleProperty = DependencyProperty.Register(
      nameof(ResizeThumbStyle),
      typeof(Style),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Style? ResizeThumbStyle
    {
      get => (Style?)GetValue(ResizeThumbStyleProperty);
      set => SetValue(ResizeThumbStyleProperty, value);
    }

    public static readonly DependencyProperty PanePlacementProperty = DependencyProperty.Register(
      nameof(PanePlacement),
      typeof(SplitViewPanePlacement),
      typeof(HamburgerMenu),
      new PropertyMetadata(SplitViewPanePlacement.Left));

    public SplitViewPanePlacement PanePlacement
    {
      get => (SplitViewPanePlacement)GetValue(PanePlacementProperty);
      set => SetValue(PanePlacementProperty, value);
    }

    public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
      nameof(DisplayMode),
      typeof(SplitViewDisplayMode),
      typeof(HamburgerMenu),
      new PropertyMetadata(SplitViewDisplayMode.CompactInline, OnDisplayModePropertyChangedCallback));

    private static void OnDisplayModePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue && e.NewValue is SplitViewDisplayMode && dependencyObject is HamburgerMenu hamburgerMenu)
      {
        hamburgerMenu.CoerceValue(OpenPaneLengthProperty);
      }
    }

    public SplitViewDisplayMode DisplayMode
    {
      get => (SplitViewDisplayMode)GetValue(DisplayModeProperty);
      set => SetValue(DisplayModeProperty, value);
    }

    public static readonly DependencyProperty PaneMarginProperty = DependencyProperty.Register(
      nameof(PaneMargin),
      typeof(Thickness),
      typeof(HamburgerMenu),
      new PropertyMetadata(new Thickness()));

    public Thickness PaneMargin
    {
      get => (Thickness)GetValue(PaneMarginProperty);
      set => SetValue(PaneMarginProperty, value);
    }

    public static readonly DependencyProperty PaneHeaderMarginProperty = DependencyProperty.Register(
      nameof(PaneHeaderMargin),
      typeof(Thickness),
      typeof(HamburgerMenu),
      new PropertyMetadata(new Thickness()));

    public Thickness PaneHeaderMargin
    {
      get => (Thickness)GetValue(PaneHeaderMarginProperty);
      set => SetValue(PaneHeaderMarginProperty, value);
    }

    public static readonly DependencyProperty PaneBackgroundProperty = DependencyProperty.Register(
      nameof(PaneBackground),
      typeof(Brush),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Brush? PaneBackground
    {
      get => (Brush?)GetValue(PaneBackgroundProperty);
      set => SetValue(PaneBackgroundProperty, value);
    }

    public static readonly DependencyProperty PaneForegroundProperty = DependencyProperty.Register(
      nameof(PaneForeground),
      typeof(Brush),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Brush? PaneForeground
    {
      get => (Brush?)GetValue(PaneForegroundProperty);
      set => SetValue(PaneForegroundProperty, value);
    }

    public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register(
      nameof(IsPaneOpen),
      typeof(bool),
      typeof(HamburgerMenu),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsPaneOpenPropertyChangedCallback));

    private static void IsPaneOpenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
      if (args.NewValue != args.OldValue)
      {
        (dependencyObject as HamburgerMenu)?.ChangeItemFocusVisualStyle();
      }
    }

    public bool IsPaneOpen
    {
      get => (bool)GetValue(IsPaneOpenProperty);
      set => SetValue(IsPaneOpenProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
      nameof(ItemsSource),
      typeof(object),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public object? ItemsSource
    {
      get => GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
      nameof(ItemContainerStyle),
      typeof(Style),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Style? ItemContainerStyle
    {
      get => (Style?)GetValue(ItemContainerStyleProperty);
      set => SetValue(ItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty HeaderItemContainerStyleProperty = DependencyProperty.Register(
      nameof(HeaderItemContainerStyle),
      typeof(Style),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Style? HeaderItemContainerStyle
    {
      get => (Style?)GetValue(HeaderItemContainerStyleProperty);
      set => SetValue(HeaderItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty SeparatorItemContainerStyleProperty = DependencyProperty.Register(
      nameof(SeparatorItemContainerStyle),
      typeof(Style),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public Style? SeparatorItemContainerStyle
    {
      get => (Style?)GetValue(SeparatorItemContainerStyleProperty);
      set => SetValue(SeparatorItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
      nameof(ItemTemplate),
      typeof(DataTemplate),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public DataTemplate? ItemTemplate
    {
      get => (DataTemplate?)GetValue(ItemTemplateProperty);
      set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
      nameof(ItemTemplateSelector),
      typeof(DataTemplateSelector),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public DataTemplateSelector? ItemTemplateSelector
    {
      get => (DataTemplateSelector?)GetValue(ItemTemplateSelectorProperty);
      set => SetValue(ItemTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
      nameof(SelectedItem),
      typeof(object),
      typeof(HamburgerMenu),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
      get => GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
      nameof(SelectedIndex),
      typeof(int),
      typeof(HamburgerMenu),
      new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal));

    public int SelectedIndex
    {
      get => (int)GetValue(SelectedIndexProperty);
      set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly DependencyProperty ContentTransitionProperty = DependencyProperty.Register(
      nameof(ContentTransition),
      typeof(TransitionType),
      typeof(HamburgerMenu),
      new FrameworkPropertyMetadata(TransitionType.Normal));

    public TransitionType ContentTransition
    {
      get => (TransitionType)GetValue(ContentTransitionProperty);
      set => SetValue(ContentTransitionProperty, value);
    }

    public static readonly DependencyProperty ItemCommandProperty = DependencyProperty.Register(
      nameof(ItemCommand),
      typeof(ICommand),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public ICommand? ItemCommand
    {
      get => (ICommand?)GetValue(ItemCommandProperty);
      set => SetValue(ItemCommandProperty, value);
    }

    public static readonly DependencyProperty ItemCommandParameterProperty = DependencyProperty.Register(
      nameof(ItemCommandParameter),
      typeof(object),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public object? ItemCommandParameter
    {
      get => GetValue(ItemCommandParameterProperty);
      set => SetValue(ItemCommandParameterProperty, value);
    }

    public static readonly DependencyProperty VerticalScrollBarOnLeftSideProperty = DependencyProperty.Register(
      nameof(VerticalScrollBarOnLeftSide),
      typeof(bool),
      typeof(HamburgerMenu),
      new PropertyMetadata(BooleanBoxes.FalseBox));

    public bool VerticalScrollBarOnLeftSide
    {
      get => (bool)GetValue(VerticalScrollBarOnLeftSideProperty);
      set => SetValue(VerticalScrollBarOnLeftSideProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty ShowSelectionIndicatorProperty = DependencyProperty.Register(
      nameof(ShowSelectionIndicator),
      typeof(bool),
      typeof(HamburgerMenu),
      new PropertyMetadata(BooleanBoxes.FalseBox));

    public bool ShowSelectionIndicator
    {
      get => (bool)GetValue(ShowSelectionIndicatorProperty);
      set => SetValue(ShowSelectionIndicatorProperty, BooleanBoxes.Box(value));
    }

    private static readonly DependencyPropertyKey ItemFocusVisualStylePropertyKey = DependencyProperty.RegisterReadOnly(
      nameof(ItemFocusVisualStyle),
      typeof(Style),
      typeof(HamburgerMenu),
      new PropertyMetadata(null));

    public static readonly DependencyProperty ItemFocusVisualStyleProperty = ItemFocusVisualStylePropertyKey.DependencyProperty;

    public Style? ItemFocusVisualStyle
    {
      get => (Style?)GetValue(ItemFocusVisualStyleProperty);
      protected set => SetValue(ItemFocusVisualStylePropertyKey, value);
    }

    public ItemCollection Items
    {
      get
      {
        if (buttonsListView is null)
        {
          throw new Exception("ButtonsListView is not defined yet. Please use ItemsSource instead.");
        }

        return buttonsListView.Items;
      }
    }

    public void RaiseItemCommand()
    {
      var command = ItemCommand;
      var commandParameter = ItemCommandParameter ?? this;
      if (command != null && command.CanExecute(commandParameter))
      {
        command.Execute(commandParameter);
      }
    }

    private void ChangeItemFocusVisualStyle()
    {
      _defaultItemFocusVisualTemplate ??= TryFindResource("Crystal.Templates.HamburgerMenuItem.FocusVisual") as ControlTemplate;
      if (_defaultItemFocusVisualTemplate != null)
      {
        var focusVisualStyle = new Style(typeof(Control));
        focusVisualStyle.Setters.Add(new Setter(TemplateProperty, _defaultItemFocusVisualTemplate));
        focusVisualStyle.Setters.Add(new Setter(WidthProperty, IsPaneOpen ? OpenPaneLength : CompactPaneLength));
        focusVisualStyle.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Left));
        focusVisualStyle.Seal();

        SetValue(ItemFocusVisualStylePropertyKey, focusVisualStyle);
      }
    }
  }
}