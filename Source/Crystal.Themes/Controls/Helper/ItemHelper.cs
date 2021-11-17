using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  public static class ItemHelper
  {
    public static readonly DependencyProperty ActiveSelectionBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "ActiveSelectionBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetActiveSelectionBackgroundBrush(UIElement element) => (Brush?)element.GetValue(ActiveSelectionBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetActiveSelectionBackgroundBrush(UIElement element, Brush? value) => element.SetValue(ActiveSelectionBackgroundBrushProperty, value);

    public static readonly DependencyProperty ActiveSelectionForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "ActiveSelectionForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetActiveSelectionForegroundBrush(UIElement element) => (Brush?)element.GetValue(ActiveSelectionForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetActiveSelectionForegroundBrush(UIElement element, Brush? value) => element.SetValue(ActiveSelectionForegroundBrushProperty, value);

    public static readonly DependencyProperty SelectedBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "SelectedBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetSelectedBackgroundBrush(UIElement element) => (Brush?)element.GetValue(SelectedBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetSelectedBackgroundBrush(UIElement element, Brush? value) => element.SetValue(SelectedBackgroundBrushProperty, value);

    public static readonly DependencyProperty SelectedForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "SelectedForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetSelectedForegroundBrush(UIElement element) => (Brush?)element.GetValue(SelectedForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetSelectedForegroundBrush(UIElement element, Brush? value) => element.SetValue(SelectedForegroundBrushProperty, value);

    public static readonly DependencyProperty HoverBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "HoverBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetHoverBackgroundBrush(UIElement element) => (Brush?)element.GetValue(HoverBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetHoverBackgroundBrush(UIElement element, Brush? value) => element.SetValue(HoverBackgroundBrushProperty, value);

    public static readonly DependencyProperty HoverForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "HoverForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetHoverForegroundBrush(UIElement element) => (Brush?)element.GetValue(HoverForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetHoverForegroundBrush(UIElement element, Brush? value) => element.SetValue(HoverForegroundBrushProperty, value);

    public static readonly DependencyProperty HoverSelectedBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "HoverSelectedBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetHoverSelectedBackgroundBrush(UIElement element) => (Brush?)element.GetValue(HoverSelectedBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetHoverSelectedBackgroundBrush(UIElement element, Brush? value) => element.SetValue(HoverSelectedBackgroundBrushProperty, value);

    public static readonly DependencyProperty HoverSelectedForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "HoverSelectedForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetHoverSelectedForegroundBrush(UIElement element) => (Brush?)element.GetValue(HoverSelectedForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetHoverSelectedForegroundBrush(UIElement element, Brush? value) => element.SetValue(HoverSelectedForegroundBrushProperty, value);

    public static readonly DependencyProperty DisabledSelectedBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "DisabledSelectedBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetDisabledSelectedBackgroundBrush(UIElement element) => (Brush?)element.GetValue(DisabledSelectedBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetDisabledSelectedBackgroundBrush(UIElement element, Brush? value) => element.SetValue(DisabledSelectedBackgroundBrushProperty, value);

    public static readonly DependencyProperty DisabledSelectedForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "DisabledSelectedForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetDisabledSelectedForegroundBrush(UIElement element) => (Brush?)element.GetValue(DisabledSelectedForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetDisabledSelectedForegroundBrush(UIElement element, Brush? value) => element.SetValue(DisabledSelectedForegroundBrushProperty, value);

    public static readonly DependencyProperty DisabledBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "DisabledBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetDisabledBackgroundBrush(UIElement element) => (Brush?)element.GetValue(DisabledBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetDisabledBackgroundBrush(UIElement element, Brush? value) => element.SetValue(DisabledBackgroundBrushProperty, value);

    public static readonly DependencyProperty DisabledForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "DisabledForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetDisabledForegroundBrush(UIElement element) => (Brush?)element.GetValue(DisabledForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetDisabledForegroundBrush(UIElement element, Brush? value) => element.SetValue(DisabledForegroundBrushProperty, value);

    private static readonly DependencyPropertyKey IsMouseLeftButtonPressedPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "IsMouseLeftButtonPressed",
      typeof(bool),
      typeof(ItemHelper),
      new PropertyMetadata(BooleanBoxes.FalseBox));

    public static readonly DependencyProperty IsMouseLeftButtonPressedProperty = IsMouseLeftButtonPressedPropertyKey.DependencyProperty;

    public static bool GetIsMouseLeftButtonPressed(UIElement element) => (bool)element.GetValue(IsMouseLeftButtonPressedProperty);

    public static readonly DependencyProperty MouseLeftButtonPressedBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "MouseLeftButtonPressedBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnMouseLeftButtonPressedBackgroundBrushPropertyChanged));

    private static void OnMouseLeftButtonPressedBackgroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is UIElement element && e.OldValue != e.NewValue)
      {
        element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        element.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        element.MouseEnter -= OnLeftMouseEnter;
        element.MouseLeave -= OnLeftMouseLeave;

        if (e.NewValue is Brush || GetMouseLeftButtonPressedForegroundBrush(element) != null)
        {
          element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
          element.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
          element.MouseEnter += OnLeftMouseEnter;
          element.MouseLeave += OnLeftMouseLeave;
        }
      }
    }

    private static void OnLeftMouseEnter(object sender, MouseEventArgs e)
    {
      var element = (UIElement)sender;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        element.SetValue(IsMouseLeftButtonPressedPropertyKey, BooleanBoxes.TrueBox);
      }
    }

    private static void OnLeftMouseLeave(object sender, MouseEventArgs e)
    {
      var element = (UIElement)sender;
      if (e.LeftButton == MouseButtonState.Pressed && GetIsMouseLeftButtonPressed(element))
      {
        element.SetValue(IsMouseLeftButtonPressedPropertyKey, BooleanBoxes.FalseBox);
      }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var element = (UIElement)sender;
      if (e.ButtonState == MouseButtonState.Pressed)
      {
        element.SetValue(IsMouseLeftButtonPressedPropertyKey, BooleanBoxes.TrueBox);
      }
    }

    private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      var element = (UIElement)sender;
      if (GetIsMouseLeftButtonPressed(element))
      {
        element.SetValue(IsMouseLeftButtonPressedPropertyKey, BooleanBoxes.FalseBox);
      }
    }

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetMouseLeftButtonPressedBackgroundBrush(UIElement element) => (Brush?)element.GetValue(MouseLeftButtonPressedBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetMouseLeftButtonPressedBackgroundBrush(UIElement element, Brush? value) => element.SetValue(MouseLeftButtonPressedBackgroundBrushProperty, value);

    public static readonly DependencyProperty MouseLeftButtonPressedForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "MouseLeftButtonPressedForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnMouseLeftButtonPressedForegroundBrushPropertyChanged));

    private static void OnMouseLeftButtonPressedForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is UIElement element && e.OldValue != e.NewValue)
      {
        element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        element.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        element.MouseEnter -= OnLeftMouseEnter;
        element.MouseLeave -= OnLeftMouseLeave;

        if (e.NewValue is Brush || GetMouseLeftButtonPressedBackgroundBrush(element) != null)
        {
          element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
          element.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
          element.MouseEnter += OnLeftMouseEnter;
          element.MouseLeave += OnLeftMouseLeave;
        }
      }
    }

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetMouseLeftButtonPressedForegroundBrush(UIElement element) => (Brush?)element.GetValue(MouseLeftButtonPressedForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetMouseLeftButtonPressedForegroundBrush(UIElement element, Brush? value) => element.SetValue(MouseLeftButtonPressedForegroundBrushProperty, value);

    private static readonly DependencyPropertyKey IsMouseRightButtonPressedPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
      "IsMouseRightButtonPressed",
      typeof(bool),
      typeof(ItemHelper),
      new PropertyMetadata(BooleanBoxes.FalseBox));

    public static readonly DependencyProperty IsMouseRightButtonPressedProperty = IsMouseRightButtonPressedPropertyKey.DependencyProperty;

    public static bool GetIsMouseRightButtonPressed(UIElement element) => (bool)element.GetValue(IsMouseRightButtonPressedProperty);

    public static readonly DependencyProperty MouseRightButtonPressedBackgroundBrushProperty = DependencyProperty.RegisterAttached(
      "MouseRightButtonPressedBackgroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnMouseRightButtonPressedBackgroundBrushPropertyChanged));

    private static void OnMouseRightButtonPressedBackgroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is UIElement element && e.OldValue != e.NewValue)
      {
        element.PreviewMouseRightButtonDown -= OnPreviewMouseRightButtonDown;
        element.PreviewMouseRightButtonUp -= OnPreviewMouseRightButtonUp;

        if (e.NewValue is Brush || GetMouseRightButtonPressedForegroundBrush(element) != null)
        {
          element.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
          element.PreviewMouseRightButtonUp += OnPreviewMouseRightButtonUp;
        }
      }
    }

    private static void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      var element = (UIElement)sender;
      if (e.ButtonState == MouseButtonState.Pressed)
      {
        if (element is TreeViewItem)
        {
          Mouse.Capture(element, CaptureMode.SubTree);
        }
        else
        {
          Mouse.Capture(element);
        }

        element.SetValue(IsMouseRightButtonPressedPropertyKey, BooleanBoxes.TrueBox);
      }
    }

    private static void OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      var element = (UIElement)sender;
      if (GetIsMouseRightButtonPressed(element))
      {
        Mouse.Capture(null);
        element.SetValue(IsMouseRightButtonPressedPropertyKey, BooleanBoxes.FalseBox);
      }
    }

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetMouseRightButtonPressedBackgroundBrush(UIElement element) => (Brush?)element.GetValue(MouseRightButtonPressedBackgroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetMouseRightButtonPressedBackgroundBrush(UIElement element, Brush? value) => element.SetValue(MouseRightButtonPressedBackgroundBrushProperty, value);

    public static readonly DependencyProperty MouseRightButtonPressedForegroundBrushProperty = DependencyProperty.RegisterAttached(
      "MouseRightButtonPressedForegroundBrush",
      typeof(Brush),
      typeof(ItemHelper),
      new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, OnMouseRightButtonPressedForegroundBrushPropertyChanged));

    private static void OnMouseRightButtonPressedForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is UIElement element && e.OldValue != e.NewValue)
      {
        element.PreviewMouseRightButtonDown -= OnPreviewMouseRightButtonDown;
        element.PreviewMouseRightButtonUp -= OnPreviewMouseRightButtonUp;

        if (e.NewValue is Brush || GetMouseRightButtonPressedBackgroundBrush(element) != null)
        {
          element.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
          element.PreviewMouseRightButtonUp += OnPreviewMouseRightButtonUp;
        }
      }
    }

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static Brush? GetMouseRightButtonPressedForegroundBrush(UIElement element) => (Brush?)element.GetValue(MouseRightButtonPressedForegroundBrushProperty);

    [Category(AppName.CrystalThemes)]
    [AttachedPropertyBrowsableForType(typeof(ListBoxItem))]
    [AttachedPropertyBrowsableForType(typeof(TreeViewItem))]
    public static void SetMouseRightButtonPressedForegroundBrush(UIElement element, Brush? value) => element.SetValue(MouseRightButtonPressedForegroundBrushProperty, value);
  }
}