// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Crystal.Themes.Automation.Peers;
using Crystal.Themes.Theming;

namespace Crystal.Themes.Controls;

[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(WindowCommands))]
public class WindowCommands : ToolBar
{
  /// <summary>Identifies the <see cref="Theme"/> dependency property.</summary>
  public static readonly DependencyProperty ThemeProperty
    = DependencyProperty.Register(nameof(Theme),
      typeof(string),
      typeof(WindowCommands),
      new PropertyMetadata(ThemeManager.BaseColorLight, OnThemePropertyChanged));

  private static void OnThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (e.NewValue != e.OldValue && e.NewValue is string baseColor)
    {
      var windowCommands = (WindowCommands)d;

      switch (baseColor)
      {
        case ThemeManager.BaseColorLightConst:
          {
            if (windowCommands.LightTemplate != null)
            {
              windowCommands.SetValue(TemplateProperty, windowCommands.LightTemplate);
            }

            break;
          }
        case ThemeManager.BaseColorDarkConst:
          {
            if (windowCommands.DarkTemplate != null)
            {
              windowCommands.SetValue(TemplateProperty, windowCommands.DarkTemplate);
            }

            break;
          }
      }
    }
  }

  /// <summary>
  /// Gets or sets the value indicating the current theme.
  /// </summary>
  public string Theme
  {
    get => (string)GetValue(ThemeProperty);
    set => SetValue(ThemeProperty, value);
  }

  /// <summary>Identifies the <see cref="LightTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty LightTemplateProperty
    = DependencyProperty.Register(nameof(LightTemplate),
      typeof(ControlTemplate),
      typeof(WindowCommands),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the value indicating the light theme ControlTemplate.
  /// </summary>
  public ControlTemplate? LightTemplate
  {
    get => (ControlTemplate?)GetValue(LightTemplateProperty);
    set => SetValue(LightTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="DarkTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty DarkTemplateProperty
    = DependencyProperty.Register(nameof(DarkTemplate),
      typeof(ControlTemplate),
      typeof(WindowCommands),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the value indicating the light theme ControlTemplate.
  /// </summary>
  public ControlTemplate? DarkTemplate
  {
    get => (ControlTemplate?)GetValue(DarkTemplateProperty);
    set => SetValue(DarkTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="ShowSeparators"/> dependency property.</summary>
  public static readonly DependencyProperty ShowSeparatorsProperty
    = DependencyProperty.Register(nameof(ShowSeparators),
      typeof(bool),
      typeof(WindowCommands),
      new FrameworkPropertyMetadata(BooleanBoxes.TrueBox,
        FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
        OnShowSeparatorsPropertyChanged));

  private static void OnShowSeparatorsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (e.NewValue != e.OldValue)
    {
      ((WindowCommands)d).ResetSeparators();
    }
  }

  /// <summary>
  /// Gets or sets the value indicating whether to show the separators or not.
  /// </summary>
  public bool ShowSeparators
  {
    get => (bool)GetValue(ShowSeparatorsProperty);
    set => SetValue(ShowSeparatorsProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="ShowLastSeparator"/> dependency property.</summary>
  public static readonly DependencyProperty ShowLastSeparatorProperty
    = DependencyProperty.Register(nameof(ShowLastSeparator),
      typeof(bool),
      typeof(WindowCommands),
      new FrameworkPropertyMetadata(BooleanBoxes.TrueBox,
        FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
        OnShowLastSeparatorPropertyChanged));

  private static void OnShowLastSeparatorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (e.NewValue != e.OldValue)
    {
      ((WindowCommands)d).ResetSeparators(false);
    }
  }

  /// <summary>
  /// Gets or sets the value indicating whether to show the last separator or not.
  /// </summary>
  public bool ShowLastSeparator
  {
    get => (bool)GetValue(ShowLastSeparatorProperty);
    set => SetValue(ShowLastSeparatorProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="SeparatorHeight"/> dependency property.</summary>
  public static readonly DependencyProperty SeparatorHeightProperty
    = DependencyProperty.Register(nameof(SeparatorHeight),
      typeof(double),
      typeof(WindowCommands),
      new FrameworkPropertyMetadata(15d,
        FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

  /// <summary>
  /// Gets or sets the value indicating the height of the separators.
  /// </summary>
  [TypeConverter(typeof(LengthConverter))]
  public double SeparatorHeight
  {
    get => (double)GetValue(SeparatorHeightProperty);
    set => SetValue(SeparatorHeightProperty, value);
  }

  /// <summary>Identifies the <see cref="ParentWindow"/> dependency property.</summary>
  internal static readonly DependencyPropertyKey ParentWindowPropertyKey =
    DependencyProperty.RegisterReadOnly(nameof(ParentWindow),
      typeof(Window),
      typeof(WindowCommands),
      new PropertyMetadata(null));

  /// <summary>Identifies the <see cref="ParentWindow"/> dependency property.</summary>
  public static readonly DependencyProperty ParentWindowProperty = ParentWindowPropertyKey.DependencyProperty;

  /// <summary>
  /// Gets the window.
  /// </summary>
  public Window? ParentWindow
  {
    get => (Window?)GetValue(ParentWindowProperty);
    protected set => SetValue(ParentWindowPropertyKey, value);
  }

  static WindowCommands()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowCommands), new FrameworkPropertyMetadata(typeof(WindowCommands)));
  }

  public WindowCommands()
  {
    Loaded += WindowCommandsLoaded;
  }

  protected override DependencyObject GetContainerForItemOverride()
  {
    return new WindowCommandsItem();
  }

  protected override bool IsItemItsOwnContainerOverride(object item)
  {
    return item is WindowCommandsItem;
  }

  protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
  {
    base.PrepareContainerForItemOverride(element, item);

    if (!(element is WindowCommandsItem windowCommandsItem))
    {
      return;
    }

    var frameworkElement = item as FrameworkElement;
    if (item is not FrameworkElement)
    {
      windowCommandsItem.ApplyTemplate();
      frameworkElement = windowCommandsItem.ContentTemplate?.LoadContent() as FrameworkElement;
    }

    frameworkElement?.SetBinding(ControlsHelper.ContentCharacterCasingProperty,
      new Binding { Source = this, Path = new PropertyPath(ControlsHelper.ContentCharacterCasingProperty) });

    AttachVisibilityHandler(windowCommandsItem, item as UIElement);
    ResetSeparators();
  }

  protected override void ClearContainerForItemOverride(DependencyObject element, object item)
  {
    base.ClearContainerForItemOverride(element, item);

    if (item is FrameworkElement frameworkElement)
    {
      BindingOperations.ClearBinding(frameworkElement, ControlsHelper.ContentCharacterCasingProperty);
    }

    DetachVisibilityHandler(element as WindowCommandsItem);
    ResetSeparators(false);
  }

  private void AttachVisibilityHandler(WindowCommandsItem? container, UIElement? item)
  {
    if (container == null)
    {
      return;
    }

    if (item is null)
    {
      // if item is not a UIElement then maybe the ItemsSource binds to a collection of objects
      // and an ItemTemplate is set, so let's try to solve this
      container.ApplyTemplate();
      if (!(container.ContentTemplate?.LoadContent() is UIElement))
      {
        // no UIElement was found, so don't show this container
        container.Visibility = Visibility.Collapsed;
      }

      return;
    }

    container.Visibility = item.Visibility;
    var isVisibilityNotifier = new PropertyChangeNotifier(item, VisibilityProperty);
    isVisibilityNotifier.ValueChanged += VisibilityPropertyChanged;
    container.VisibilityPropertyChangeNotifier = isVisibilityNotifier;
  }

  private void DetachVisibilityHandler(WindowCommandsItem? container)
  {
    if (container != null)
    {
      container.VisibilityPropertyChangeNotifier = null;
    }
  }

  private void VisibilityPropertyChanged(object? sender, EventArgs e)
  {
    if (sender is UIElement item)
    {
      var container = GetWindowCommandsItem(item);
      if (container != null)
      {
        container.Visibility = item.Visibility;
        ResetSeparators();
      }
    }
  }

  protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
  {
    base.OnItemsChanged(e);

    ResetSeparators();
  }

  private void ResetSeparators(bool reset = true)
  {
    if (Items.Count == 0)
    {
      return;
    }

    var windowCommandsItems = GetWindowCommandsItems().ToList();

    if (reset)
    {
      foreach (var windowCommandsItem in windowCommandsItems)
      {
        windowCommandsItem.IsSeparatorVisible = ShowSeparators;
      }
    }

    var lastContainer = windowCommandsItems.LastOrDefault(i => i.IsVisible);
    if (lastContainer != null)
    {
      lastContainer.IsSeparatorVisible = ShowSeparators && ShowLastSeparator;
    }
  }

  private WindowCommandsItem? GetWindowCommandsItem(object? item)
  {
    if (item is WindowCommandsItem windowCommandsItem)
    {
      return windowCommandsItem;
    }

    return (WindowCommandsItem?)ItemContainerGenerator.ContainerFromItem(item);
  }

  private IEnumerable<WindowCommandsItem> GetWindowCommandsItems()
  {
    foreach (var item in Items)
    {
      var windowCommandsItem = GetWindowCommandsItem(item);
      if (windowCommandsItem != null)
      {
        yield return windowCommandsItem;
      }
    }
  }

  private void WindowCommandsLoaded(object sender, RoutedEventArgs e)
  {
    Loaded -= WindowCommandsLoaded;

    var contentPresenter = this.TryFindParent<ContentPresenter>();
    if (contentPresenter != null)
    {
      SetCurrentValue(DockPanel.DockProperty, contentPresenter.GetValue(DockPanel.DockProperty));
    }

    if (null == ParentWindow)
    {
      var window = this.TryFindParent<Window>();
      SetValue(ParentWindowPropertyKey, window);
    }
  }

  /// <summary>
  /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
  /// </summary>
  protected override AutomationPeer OnCreateAutomationPeer()
  {
    return new WindowCommandsAutomationPeer(this);
  }
}