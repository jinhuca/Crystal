namespace Crystal.Themes.Controls
{
  public enum BadgePlacementMode
  {
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
  }

  [TemplatePart(Name = BadgeContainerPartName, Type = typeof(UIElement))]
  public class BadgedEx : ContentControl
  {
    public const string BadgeContainerPartName = "PART_BadgeContainer";
    [CLSCompliant(false)]

    protected FrameworkElement? _badgeContainer;
    public static readonly DependencyProperty BadgeProperty = DependencyProperty.Register(
      nameof(Badge),
      typeof(object),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsArrange, OnBadgeChanged));

    public object Badge
    {
      get => GetValue(BadgeProperty);
      set => SetValue(BadgeProperty, value);
    }

    public static readonly DependencyProperty BadgeFontFamilyProperty = DependencyProperty.RegisterAttached(
      nameof(BadgeFontFamily),
      typeof(FontFamily),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Bindable(true)]
    [Localizability(LocalizationCategory.Font)]
    public FontFamily BadgeFontFamily
    {
      get => (FontFamily)GetValue(BadgeFontFamilyProperty);
      set => SetValue(BadgeFontFamilyProperty, value);
    }

    public static readonly DependencyProperty BadgeFontStyleProperty = DependencyProperty.RegisterAttached(
      nameof(BadgeFontStyle),
      typeof(FontStyle),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Bindable(true)]
    public FontStyle BadgeFontStyle
    {
      get => (FontStyle)GetValue(BadgeFontStyleProperty);
      set => SetValue(BadgeFontStyleProperty, value);
    }

    public static readonly DependencyProperty BadgeFontWeightProperty = DependencyProperty.RegisterAttached(
      nameof(BadgeFontWeight),
      typeof(FontWeight),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Bindable(true)]
    public FontWeight BadgeFontWeight
    {
      get => (FontWeight)GetValue(BadgeFontWeightProperty);
      set => SetValue(BadgeFontWeightProperty, value);
    }

    public static readonly DependencyProperty BadgeFontStretchProperty = DependencyProperty.RegisterAttached(
      nameof(BadgeFontStretch),
      typeof(FontStretch),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(FontStretches.Normal,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Bindable(true)]
    public FontStretch BadgeFontStretch
    {
      get => (FontStretch)GetValue(BadgeFontStretchProperty);
      set => SetValue(BadgeFontStretchProperty, value);
    }

    public static readonly DependencyProperty BadgeFontSizeProperty = DependencyProperty.RegisterAttached(
      nameof(BadgeFontSize),
      typeof(double),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(SystemFonts.MessageFontSize,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [TypeConverter(typeof(FontSizeConverter))]
    [Localizability(LocalizationCategory.None)]
    public double BadgeFontSize
    {
      get => (double)GetValue(BadgeFontSizeProperty);
      set => SetValue(BadgeFontSizeProperty, value);
    }

    public static readonly DependencyProperty BadgeBackgroundProperty = DependencyProperty.Register(
      nameof(BadgeBackground),
      typeof(Brush),
      typeof(BadgedEx),
      new PropertyMetadata(default(Brush)));

    public Brush? BadgeBackground
    {
      get => (Brush?)GetValue(BadgeBackgroundProperty);
      set => SetValue(BadgeBackgroundProperty, value);
    }

    public static readonly DependencyProperty BadgeForegroundProperty = DependencyProperty.Register(
      nameof(BadgeForeground),
      typeof(Brush),
      typeof(BadgedEx),
      new PropertyMetadata(default(Brush)));

    public Brush? BadgeForeground
    {
      get => (Brush?)GetValue(BadgeForegroundProperty);
      set => SetValue(BadgeForegroundProperty, value);
    }

    public static readonly DependencyProperty BadgeBorderBrushProperty = DependencyProperty.Register(
      nameof(BadgeBorderBrush),
      typeof(Brush),
      typeof(BadgedEx),
      new PropertyMetadata(default(Brush)));

    public Brush? BadgeBorderBrush
    {
      get => (Brush?)GetValue(BadgeBorderBrushProperty);
      set => SetValue(BadgeBorderBrushProperty, value);
    }

    public static readonly DependencyProperty BadgeBorderThicknessProperty = DependencyProperty.Register(
      nameof(BadgeBorderThickness),
      typeof(Thickness),
      typeof(BadgedEx),
      new PropertyMetadata(default(Thickness)));

    public Thickness BadgeBorderThickness
    {
      get => (Thickness)GetValue(BadgeBorderThicknessProperty);
      set => SetValue(BadgeBorderThicknessProperty, value);
    }

    public static readonly DependencyProperty BadgePlacementModeProperty = DependencyProperty.Register(
      nameof(BadgePlacementMode),
      typeof(BadgePlacementMode),
      typeof(BadgedEx),
      new PropertyMetadata(default(BadgePlacementMode)));

    public BadgePlacementMode BadgePlacementMode
    {
      get => (BadgePlacementMode)GetValue(BadgePlacementModeProperty);
      set => SetValue(BadgePlacementModeProperty, value);
    }

    public static readonly DependencyProperty BadgeMarginProperty = DependencyProperty.Register(
      nameof(BadgeMargin),
      typeof(Thickness),
      typeof(BadgedEx),
      new PropertyMetadata(default(Thickness)));

    public Thickness BadgeMargin
    {
      get => (Thickness)GetValue(BadgeMarginProperty);
      set => SetValue(BadgeMarginProperty, value);
    }

    public static readonly DependencyProperty BadgeTemplateProperty = DependencyProperty.Register(
      nameof(BadgeTemplate),
      typeof(DataTemplate),
      typeof(BadgedEx),
      new PropertyMetadata(null));

    public DataTemplate? BadgeTemplate
    {
      get => (DataTemplate?)GetValue(BadgeTemplateProperty);
      set => SetValue(BadgeTemplateProperty, value);
    }

    public static readonly DependencyProperty BadgeTemplateSelectorProperty = DependencyProperty.Register(
      nameof(BadgeTemplateSelector),
      typeof(DataTemplateSelector),
      typeof(BadgedEx),
      new PropertyMetadata(null));

    public DataTemplateSelector? BadgeTemplateSelector
    {
      get => (DataTemplateSelector?)GetValue(BadgeTemplateSelectorProperty);
      set => SetValue(BadgeTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty BadgeStringFormatProperty = DependencyProperty.Register(
      nameof(BadgeStringFormat),
      typeof(string),
      typeof(BadgedEx),
      new FrameworkPropertyMetadata(default(string)));

    public string? BadgeStringFormat
    {
      get => (string?)GetValue(BadgeStringFormatProperty);
      set => SetValue(BadgeStringFormatProperty, value);
    }

    public static readonly RoutedEvent BadgeChangedEvent = EventManager.RegisterRoutedEvent(
      nameof(BadgeChanged),
      RoutingStrategy.Bubble,
      typeof(RoutedPropertyChangedEventHandler<object>),
      typeof(BadgedEx));

    public event RoutedPropertyChangedEventHandler<object> BadgeChanged
    {
      add => AddHandler(BadgeChangedEvent, value);
      remove => RemoveHandler(BadgeChangedEvent, value);
    }

    private static readonly DependencyPropertyKey IsBadgeSetPropertyKey = DependencyProperty.RegisterReadOnly(
      nameof(IsBadgeSet),
      typeof(bool),
      typeof(BadgedEx),
      new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsBadgeSetProperty = IsBadgeSetPropertyKey.DependencyProperty;

    public bool IsBadgeSet
    {
      get => (bool)GetValue(IsBadgeSetProperty);
      private set => SetValue(IsBadgeSetPropertyKey, value);
    }

    private static void OnBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var instance = (BadgedEx)d;
      instance.IsBadgeSet = !string.IsNullOrWhiteSpace(e.NewValue as string) || (e.NewValue is not null && !(e.NewValue is string));

      var args = new RoutedPropertyChangedEventArgs<object?>(e.OldValue, e.NewValue) { RoutedEvent = BadgeChangedEvent };
      instance.RaiseEvent(args);
    }

    static BadgedEx()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BadgedEx), new FrameworkPropertyMetadata(typeof(BadgedEx)));
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      _badgeContainer = GetTemplateChild(BadgeContainerPartName) as FrameworkElement;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      var result = base.ArrangeOverride(arrangeBounds);

      if (_badgeContainer is null)
      {
        return result;
      }

      var containerDesiredSize = _badgeContainer.DesiredSize;
      if ((containerDesiredSize.Width <= 0.0 || containerDesiredSize.Height <= 0.0)
          && !double.IsNaN(_badgeContainer.ActualWidth)
          && !double.IsInfinity(_badgeContainer.ActualWidth)
          && !double.IsNaN(_badgeContainer.ActualHeight)
          && !double.IsInfinity(_badgeContainer.ActualHeight))
      {
        containerDesiredSize = new Size(_badgeContainer.ActualWidth, _badgeContainer.ActualHeight);
      }

      var h = 0 - (containerDesiredSize.Width / 2);
      var v = 0 - (containerDesiredSize.Height / 2);
      _badgeContainer.Margin = new Thickness(0);
      _badgeContainer.Margin = new Thickness(h, v, h, v);

      return result;
    }
  }
}