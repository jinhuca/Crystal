﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls;

[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_ColorPaletteStandard", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_ColorPaletteAvailable", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_ColorPaletteCustom01", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_ColorPaletteCustom02", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_ColorPaletteRecent", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_PopupTabControl", Type = typeof(TabControl))]
[TemplatePart(Name = "PART_ColorPalettesTab", Type = typeof(TabItem))]
[TemplatePart(Name = "PART_AdvancedTab", Type = typeof(TabItem))]
[StyleTypedProperty(Property = nameof(StandardColorPaletteStyle), StyleTargetType = typeof(ColorPalette))]
[StyleTypedProperty(Property = nameof(AvailableColorPaletteStyle), StyleTargetType = typeof(ColorPalette))]
[StyleTypedProperty(Property = nameof(CustomColorPalette01Style), StyleTargetType = typeof(ColorPalette))]
[StyleTypedProperty(Property = nameof(RecentColorPaletteStyle), StyleTargetType = typeof(ColorPalette))]
[StyleTypedProperty(Property = nameof(CustomColorPalette02Style), StyleTargetType = typeof(ColorPalette))]
[StyleTypedProperty(Property = nameof(TabControlStyle), StyleTargetType = typeof(TabControl))]
[StyleTypedProperty(Property = nameof(TabItemStyle), StyleTargetType = typeof(TabItem))]
public class ColorPicker : ColorPickerBase
{
  private Popup? PART_Popup;
  private ColorPalette? PART_ColorPaletteStandard;
  private ColorPalette? PART_ColorPaletteAvailable;
  private ColorPalette? PART_ColorPaletteCustom01;
  private ColorPalette? PART_ColorPaletteCustom02;
  private ColorPalette? PART_ColorPaletteRecent;
  private TabControl? PART_PopupTabControl;
  private TabItem? PART_ColorPalettesTab;
  private TabItem? PART_AdvancedTab;

  /// <summary>Identifies the <see cref="DropDownClosed"/> routed event.</summary>
  public static readonly RoutedEvent DropDownClosedEvent
    = EventManager.RegisterRoutedEvent(nameof(DropDownClosed),
      RoutingStrategy.Bubble,
      typeof(EventHandler<EventArgs>),
      typeof(ColorPicker));

  /// <summary>
  ///     Occurs when the DropDown is closed.
  /// </summary>
  public event EventHandler<EventArgs> DropDownClosed
  {
    add => AddHandler(DropDownClosedEvent, value);
    remove => RemoveHandler(DropDownClosedEvent, value);
  }

  /// <summary>Identifies the <see cref="DropDownOpened"/> routed event.</summary>
  public static readonly RoutedEvent DropDownOpenedEvent
    = EventManager.RegisterRoutedEvent(nameof(DropDownOpened),
      RoutingStrategy.Bubble,
      typeof(EventHandler<EventArgs>),
      typeof(ColorPicker));

  /// <summary>
  ///     Occurs when the DropDown is opened.
  /// </summary>
  public event EventHandler<EventArgs> DropDownOpened
  {
    add => AddHandler(DropDownOpenedEvent, value);
    remove => RemoveHandler(DropDownOpenedEvent, value);
  }

  /// <summary>Identifies the <see cref="DropDownHeight"/> dependency property.</summary>
  public static readonly DependencyProperty DropDownHeightProperty
    = DependencyProperty.Register(nameof(DropDownHeight),
      typeof(double),
      typeof(ColorPicker),
      new PropertyMetadata(300d));

  /// <summary>
  /// Gets or sets the height of the DropDown.
  /// </summary>
  public double DropDownHeight
  {
    get => (double)GetValue(DropDownHeightProperty);
    set => SetValue(DropDownHeightProperty, value);
  }

  /// <summary>Identifies the <see cref="DropDownWidth"/> dependency property.</summary>
  public static readonly DependencyProperty DropDownWidthProperty
    = DependencyProperty.Register(nameof(DropDownWidth),
      typeof(double),
      typeof(ColorPicker),
      new PropertyMetadata(300d));

  /// <summary>
  /// Gets or sets the width of the DropDown.
  /// </summary>
  [Bindable(true), Category("Layout")]
  [TypeConverter(typeof(LengthConverter))]
  public double DropDownWidth
  {
    get => (double)GetValue(DropDownWidthProperty);
    set => SetValue(DropDownWidthProperty, value);
  }

  /// <summary>Identifies the <see cref="IsDropDownOpen"/> dependency property.</summary>
  public static readonly DependencyProperty IsDropDownOpenProperty
    = DependencyProperty.Register(nameof(IsDropDownOpen),
      typeof(bool),
      typeof(ColorPicker),
      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

  /// <summary>
  /// Whether or not the "popup" for this control is currently open
  /// </summary>
  [Bindable(true), Browsable(false), Category("Appearance")]
  public bool IsDropDownOpen
  {
    get => (bool)GetValue(IsDropDownOpenProperty);
    set => SetValue(IsDropDownOpenProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="SelectedColorTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty SelectedColorTemplateProperty
    = DependencyProperty.Register(nameof(SelectedColorTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="DataTemplate"/> for the <see cref="ColorPickerBase.SelectedColor"/>
  /// </summary>
  public DataTemplate? SelectedColorTemplate
  {
    get => (DataTemplate?)GetValue(SelectedColorTemplateProperty);
    set => SetValue(SelectedColorTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="AddToRecentColorsTrigger"/> dependency property.</summary>
  public static readonly DependencyProperty AddToRecentColorsTriggerProperty
    = DependencyProperty.Register(nameof(AddToRecentColorsTrigger),
      typeof(AddToRecentColorsTrigger),
      typeof(ColorPicker),
      new PropertyMetadata(AddToRecentColorsTrigger.ColorPickerClosed));

  /// <summary>
  /// Gets or sets when to add the <see cref="ColorPickerBase.SelectedColor"/> to the <see cref="RecentColorPaletteItemsSource"/>
  /// </summary>
  public AddToRecentColorsTrigger AddToRecentColorsTrigger
  {
    get => (AddToRecentColorsTrigger)GetValue(AddToRecentColorsTriggerProperty);
    set => SetValue(AddToRecentColorsTriggerProperty, value);
  }

  /// <summary>Identifies the <see cref="IsAvailableColorPaletteVisible"/> dependency property.</summary>
  public static readonly DependencyProperty IsAvailableColorPaletteVisibleProperty =
    DependencyProperty.Register(nameof(IsAvailableColorPaletteVisible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.TrueBox));

  /// <summary>
  /// Gets or sets the visibility of the available <see cref="ColorPalette"/>.
  /// </summary>
  public bool IsAvailableColorPaletteVisible
  {
    get => (bool)GetValue(IsAvailableColorPaletteVisibleProperty);
    set => SetValue(IsAvailableColorPaletteVisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="AvailableColorPaletteHeader"/> dependency property.</summary>
  public static readonly DependencyProperty AvailableColorPaletteHeaderProperty =
    DependencyProperty.Register(nameof(AvailableColorPaletteHeader),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Available"));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.Header"/> of the available <see cref="ColorPalette"/>.
  /// </summary>
  public object AvailableColorPaletteHeader
  {
    get => GetValue(AvailableColorPaletteHeaderProperty);
    set => SetValue(AvailableColorPaletteHeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="AvailableColorPaletteHeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty AvailableColorPaletteHeaderTemplateProperty =
    DependencyProperty.Register(nameof(AvailableColorPaletteHeaderTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(default(DataTemplate)));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.HeaderTemplate"/> of the available <see cref="ColorPalette"/>.
  /// </summary>
  public DataTemplate? AvailableColorPaletteHeaderTemplate
  {
    get => (DataTemplate?)GetValue(AvailableColorPaletteHeaderTemplateProperty);
    set => SetValue(AvailableColorPaletteHeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="AvailableColorPaletteItemsSource"/> dependency property.</summary>
  public static readonly DependencyProperty AvailableColorPaletteItemsSourceProperty =
    DependencyProperty.Register(nameof(AvailableColorPaletteItemsSource),
      typeof(IEnumerable),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ItemsControl.ItemsSource"/> of the available <see cref="ColorPalette"/>.
  /// </summary>
  public IEnumerable? AvailableColorPaletteItemsSource
  {
    get => (IEnumerable?)GetValue(AvailableColorPaletteItemsSourceProperty);
    set => SetValue(AvailableColorPaletteItemsSourceProperty, value);
  }

  /// <summary>Identifies the <see cref="AvailableColorPaletteStyle"/> dependency property.</summary>
  public static readonly DependencyProperty AvailableColorPaletteStyleProperty =
    DependencyProperty.Register(nameof(AvailableColorPaletteStyle),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="FrameworkElement.Style"/> of the available <see cref="ColorPalette"/>.
  /// </summary>
  public Style? AvailableColorPaletteStyle
  {
    get => (Style?)GetValue(AvailableColorPaletteStyleProperty);
    set => SetValue(AvailableColorPaletteStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="IsCustomColorPalette01Visible"/> dependency property.</summary>
  public static readonly DependencyProperty IsCustomColorPalette01VisibleProperty =
    DependencyProperty.Register(nameof(IsCustomColorPalette01Visible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets or sets the visibility of the custom <see cref="ColorPalette"/> (1/2).
  /// </summary>
  public bool IsCustomColorPalette01Visible
  {
    get => (bool)GetValue(IsCustomColorPalette01VisibleProperty);
    set => SetValue(IsCustomColorPalette01VisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="CustomColorPalette01Header"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette01HeaderProperty =
    DependencyProperty.Register(nameof(CustomColorPalette01Header),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Custom 01"));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.Header"/> of the custom <see cref="ColorPalette"/> (1/2).
  /// </summary>
  public object CustomColorPalette01Header
  {
    get => GetValue(CustomColorPalette01HeaderProperty);
    set => SetValue(CustomColorPalette01HeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="CustomColorPalette01HeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette01HeaderTemplateProperty =
    DependencyProperty.Register(nameof(CustomColorPalette01HeaderTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.HeaderTemplate"/> of the custom <see cref="ColorPalette"/> (1/2).
  /// </summary>
  public DataTemplate? CustomColorPalette01HeaderTemplate
  {
    get => (DataTemplate?)GetValue(CustomColorPalette01HeaderTemplateProperty);
    set => SetValue(CustomColorPalette01HeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="CustomColorPalette01ItemsSource"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette01ItemsSourceProperty =
    DependencyProperty.Register(nameof(CustomColorPalette01ItemsSource),
      typeof(IEnumerable),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ItemsControl.ItemsSource"/> of the custom <see cref="ColorPalette"/> (1/2).
  /// </summary>
  public IEnumerable? CustomColorPalette01ItemsSource
  {
    get => (IEnumerable?)GetValue(CustomColorPalette01ItemsSourceProperty);
    set => SetValue(CustomColorPalette01ItemsSourceProperty, value);
  }

  /// <summary>Identifies the <see cref="CustomColorPalette01Style"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette01StyleProperty =
    DependencyProperty.Register(nameof(CustomColorPalette01Style),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="FrameworkElement.Style"/> of the custom <see cref="ColorPalette"/> (1/2).
  /// </summary>
  public Style? CustomColorPalette01Style
  {
    get => (Style?)GetValue(CustomColorPalette01StyleProperty);
    set => SetValue(CustomColorPalette01StyleProperty, value);
  }

  /// <summary>Identifies the <see cref="IsCustomColorPalette02Visible"/> dependency property.</summary>
  public static readonly DependencyProperty IsCustomColorPalette02VisibleProperty =
    DependencyProperty.Register(nameof(IsCustomColorPalette02Visible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets or sets the visibility of the custom <see cref="ColorPalette"/> (2/2).
  /// </summary>
  public bool IsCustomColorPalette02Visible
  {
    get => (bool)GetValue(IsCustomColorPalette02VisibleProperty);
    set => SetValue(IsCustomColorPalette02VisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="CustomColorPalette02Header"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette02HeaderProperty =
    DependencyProperty.Register(nameof(CustomColorPalette02Header),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Custom 02"));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.Header"/> of the custom <see cref="ColorPalette"/> (2/2).
  /// </summary>
  public object CustomColorPalette02Header
  {
    get => GetValue(CustomColorPalette02HeaderProperty);
    set => SetValue(CustomColorPalette02HeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="CustomColorPalette02HeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette02HeaderTemplateProperty =
    DependencyProperty.Register(nameof(CustomColorPalette02HeaderTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.HeaderTemplate"/> of the custom <see cref="ColorPalette"/> (2/2).
  /// </summary>
  public DataTemplate? CustomColorPalette02HeaderTemplate
  {
    get => (DataTemplate?)GetValue(CustomColorPalette02HeaderTemplateProperty);
    set => SetValue(CustomColorPalette02HeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="CustomColorPalette02ItemsSource"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette02ItemsSourceProperty =
    DependencyProperty.Register(nameof(CustomColorPalette02ItemsSource),
      typeof(IEnumerable),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ItemsControl.ItemsSource"/> of the custom <see cref="ColorPalette"/> (2/2).
  /// </summary>
  public IEnumerable? CustomColorPalette02ItemsSource
  {
    get => (IEnumerable?)GetValue(CustomColorPalette02ItemsSourceProperty);
    set => SetValue(CustomColorPalette02ItemsSourceProperty, value);
  }

  /// <summary>Identifies the <see cref="CustomColorPalette02Style"/> dependency property.</summary>
  public static readonly DependencyProperty CustomColorPalette02StyleProperty =
    DependencyProperty.Register(nameof(CustomColorPalette02Style),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="FrameworkElement.Style"/> of the custom <see cref="ColorPalette"/> (2/2).
  /// </summary>
  public Style? CustomColorPalette02Style
  {
    get => (Style?)GetValue(CustomColorPalette02StyleProperty);
    set => SetValue(CustomColorPalette02StyleProperty, value);
  }

  /// <summary>Identifies the <see cref="IsRecentColorPaletteVisible"/> dependency property.</summary>
  public static readonly DependencyProperty IsRecentColorPaletteVisibleProperty =
    DependencyProperty.Register(nameof(IsRecentColorPaletteVisible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.TrueBox));

  /// <summary>
  /// Gets or sets the visibility of the recent <see cref="ColorPalette"/>.
  /// </summary>
  public bool IsRecentColorPaletteVisible
  {
    get => (bool)GetValue(IsRecentColorPaletteVisibleProperty);
    set => SetValue(IsRecentColorPaletteVisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="RecentColorPaletteHeader"/> dependency property.</summary>
  public static readonly DependencyProperty RecentColorPaletteHeaderProperty =
    DependencyProperty.Register(nameof(RecentColorPaletteHeader),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Recent"));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.Header"/> of the recent <see cref="ColorPalette"/>.
  /// </summary>
  public object RecentColorPaletteHeader
  {
    get => GetValue(RecentColorPaletteHeaderProperty);
    set => SetValue(RecentColorPaletteHeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="RecentColorPaletteHeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty RecentColorPaletteHeaderTemplateProperty =
    DependencyProperty.Register(nameof(RecentColorPaletteHeaderTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.HeaderTemplate"/> of the recent <see cref="ColorPalette"/>.
  /// </summary>
  public DataTemplate? RecentColorPaletteHeaderTemplate
  {
    get => (DataTemplate?)GetValue(RecentColorPaletteHeaderTemplateProperty);
    set => SetValue(RecentColorPaletteHeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="RecentColorPaletteItemsSource"/> dependency property.</summary>
  public static readonly DependencyProperty RecentColorPaletteItemsSourceProperty =
    DependencyProperty.Register(nameof(RecentColorPaletteItemsSource),
      typeof(IEnumerable),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ItemsControl.ItemsSource"/> of the recent <see cref="ColorPalette"/>.
  /// </summary>
  public IEnumerable? RecentColorPaletteItemsSource
  {
    get => (IEnumerable?)GetValue(RecentColorPaletteItemsSourceProperty);
    set => SetValue(RecentColorPaletteItemsSourceProperty, value);
  }

  /// <summary>Identifies the <see cref="RecentColorPaletteStyle"/> dependency property.</summary>
  public static readonly DependencyProperty RecentColorPaletteStyleProperty =
    DependencyProperty.Register(nameof(RecentColorPaletteStyle),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="FrameworkElement.Style"/> of the recent <see cref="ColorPalette"/>.
  /// </summary>
  public Style? RecentColorPaletteStyle
  {
    get => (Style?)GetValue(RecentColorPaletteStyleProperty);
    set => SetValue(RecentColorPaletteStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="IsStandardColorPaletteVisible"/> dependency property.</summary>
  public static readonly DependencyProperty IsStandardColorPaletteVisibleProperty =
    DependencyProperty.Register(nameof(IsStandardColorPaletteVisible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.TrueBox));

  /// <summary>
  /// Gets or sets the visibility of the standard <see cref="ColorPalette"/>.
  /// </summary>
  public bool IsStandardColorPaletteVisible
  {
    get => (bool)GetValue(IsStandardColorPaletteVisibleProperty);
    set => SetValue(IsStandardColorPaletteVisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="StandardColorPaletteHeader"/> dependency property.</summary>
  public static readonly DependencyProperty StandardColorPaletteHeaderProperty =
    DependencyProperty.Register(nameof(StandardColorPaletteHeader),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Standard"));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.Header"/> of the standard <see cref="ColorPalette"/>.
  /// </summary>
  public object StandardColorPaletteHeader
  {
    get => GetValue(StandardColorPaletteHeaderProperty);
    set => SetValue(StandardColorPaletteHeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="StandardColorPaletteHeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty StandardColorPaletteHeaderTemplateProperty =
    DependencyProperty.Register(nameof(StandardColorPaletteHeaderTemplate),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ColorPalette.HeaderTemplate"/> of the standard <see cref="ColorPalette"/>.
  /// </summary>
  public object? StandardColorPaletteHeaderTemplate
  {
    get => GetValue(StandardColorPaletteHeaderTemplateProperty);
    set => SetValue(StandardColorPaletteHeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="StandardColorPaletteItemsSource"/> dependency property.</summary>
  public static readonly DependencyProperty StandardColorPaletteItemsSourceProperty =
    DependencyProperty.Register(nameof(StandardColorPaletteItemsSource),
      typeof(IEnumerable),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="ItemsControl.ItemsSource"/> of the standard <see cref="ColorPalette"/>.
  /// </summary>
  public IEnumerable? StandardColorPaletteItemsSource
  {
    get => (IEnumerable?)GetValue(StandardColorPaletteItemsSourceProperty);
    set => SetValue(StandardColorPaletteItemsSourceProperty, value);
  }

  /// <summary>Identifies the <see cref="StandardColorPaletteStyle"/> dependency property.</summary>
  public static readonly DependencyProperty StandardColorPaletteStyleProperty =
    DependencyProperty.Register(nameof(StandardColorPaletteStyle),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="FrameworkElement.Style"/> of the standard <see cref="ColorPalette"/>.
  /// </summary>
  public Style? StandardColorPaletteStyle
  {
    get => (Style?)GetValue(StandardColorPaletteStyleProperty);
    set => SetValue(StandardColorPaletteStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="TabControlStyle"/> dependency property.</summary>
  public static readonly DependencyProperty TabControlStyleProperty
    = DependencyProperty.Register(nameof(TabControlStyle),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="Style"/> for the <see cref="TabControl"/>.
  /// </summary>
  public Style? TabControlStyle
  {
    get => (Style?)GetValue(TabControlStyleProperty);
    set => SetValue(TabControlStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="TabItemStyle"/> dependency property.</summary>
  public static readonly DependencyProperty TabItemStyleProperty
    = DependencyProperty.Register(nameof(TabItemStyle),
      typeof(Style),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="Style"/> for the <see cref="TabItem"/>
  /// </summary>
  public Style? TabItemStyle
  {
    get => (Style?)GetValue(TabItemStyleProperty);
    set => SetValue(TabItemStyleProperty, value);
  }

  /// <summary>Identifies the <see cref="ColorPalettesTabHeader"/> dependency property.</summary>
  public static readonly DependencyProperty ColorPalettesTabHeaderProperty
    = DependencyProperty.Register(nameof(ColorPalettesTabHeader),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Palettes"));

  /// <summary>
  /// Gets or sets the <see cref="HeaderedContentControl.Header"/> for the <see cref="ColorPalette"/> <see cref="TabItem"/>.
  /// </summary>
  public object ColorPalettesTabHeader
  {
    get => GetValue(ColorPalettesTabHeaderProperty);
    set => SetValue(ColorPalettesTabHeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="ColorPalettesTabHeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty ColorPalettesTabHeaderTemplateProperty
    = DependencyProperty.Register(nameof(ColorPalettesTabHeaderTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="HeaderedContentControl.HeaderTemplate"/> for the <see cref="ColorPalette"/> <see cref="TabItem"/>.
  /// </summary>
  public DataTemplate? ColorPalettesTabHeaderTemplate
  {
    get => (DataTemplate?)GetValue(ColorPalettesTabHeaderTemplateProperty);
    set => SetValue(ColorPalettesTabHeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="IsColorPalettesTabVisible"/> dependency property.</summary>
  public static readonly DependencyProperty IsColorPalettesTabVisibleProperty
    = DependencyProperty.Register(nameof(IsColorPalettesTabVisible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.TrueBox, OnIsTabVisiblePropertyChanged));

  /// <summary>
  /// Gets or sets the visibility of the <see cref="ColorPalette"/> <see cref="TabItem"/>.
  /// </summary>
  public bool IsColorPalettesTabVisible
  {
    get => (bool)GetValue(IsColorPalettesTabVisibleProperty);
    set => SetValue(IsColorPalettesTabVisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="AdvancedTabHeader"/> dependency property.</summary>
  public static readonly DependencyProperty AdvancedTabHeaderProperty
    = DependencyProperty.Register(nameof(AdvancedTabHeader),
      typeof(object),
      typeof(ColorPicker),
      new PropertyMetadata("Advanced"));

  /// <summary>
  /// Gets or sets the <see cref="HeaderedContentControl.Header"/> for the <see cref="ColorCanvas"/> <see cref="TabItem"/>.
  /// </summary>
  public object AdvancedTabHeader
  {
    get => GetValue(AdvancedTabHeaderProperty);
    set => SetValue(AdvancedTabHeaderProperty, value);
  }

  /// <summary>Identifies the <see cref="AdvancedTabHeaderTemplate"/> dependency property.</summary>
  public static readonly DependencyProperty AdvancedTabHeaderTemplateProperty
    = DependencyProperty.Register(nameof(AdvancedTabHeaderTemplate),
      typeof(DataTemplate),
      typeof(ColorPicker),
      new PropertyMetadata(null));

  /// <summary>
  /// Gets or sets the <see cref="HeaderedContentControl.HeaderTemplate"/> for the <see cref="ColorCanvas"/> <see cref="TabItem"/>.
  /// </summary>
  public DataTemplate? AdvancedTabHeaderTemplate
  {
    get => (DataTemplate?)GetValue(AdvancedTabHeaderTemplateProperty);
    set => SetValue(AdvancedTabHeaderTemplateProperty, value);
  }

  /// <summary>Identifies the <see cref="IsAdvancedTabVisible"/> dependency property.</summary>
  public static readonly DependencyProperty IsAdvancedTabVisibleProperty
    = DependencyProperty.Register(nameof(IsAdvancedTabVisible),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.TrueBox, OnIsTabVisiblePropertyChanged));

  /// <summary>
  /// Gets or sets the visibility of the <see cref="ColorCanvas"/> <see cref="TabItem"/>.
  /// </summary>
  public bool IsAdvancedTabVisible
  {
    get => (bool)GetValue(IsAdvancedTabVisibleProperty);
    set => SetValue(IsAdvancedTabVisibleProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="CloseOnSelectedColorChanged"/> dependency property.</summary>
  public static readonly DependencyProperty CloseOnSelectedColorChangedProperty =
    DependencyProperty.Register(nameof(CloseOnSelectedColorChanged),
      typeof(bool),
      typeof(ColorPicker),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets or sets whether the DropDown should close after a color was selected from a <see cref="ColorPalette"/>. The default is <see langword="false"/>
  /// </summary>
  public bool CloseOnSelectedColorChanged
  {
    get => (bool)GetValue(CloseOnSelectedColorChangedProperty);
    set => SetValue(CloseOnSelectedColorChangedProperty, BooleanBoxes.Box(value));
  }

  public override void OnApplyTemplate()
  {
    if (PART_ColorPaletteStandard != null)
    {
      PART_ColorPaletteStandard.SelectionChanged -= ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteAvailable != null)
    {
      PART_ColorPaletteAvailable.SelectionChanged -= ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteCustom01 != null)
    {
      PART_ColorPaletteCustom01.SelectionChanged -= ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteCustom02 != null)
    {
      PART_ColorPaletteCustom02.SelectionChanged -= ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteRecent != null)
    {
      PART_ColorPaletteRecent.SelectionChanged -= ColorPalette_SelectionChanged;
    }

    base.OnApplyTemplate();

    PART_Popup = GetTemplateChild(nameof(PART_Popup)) as Popup;

    PART_ColorPaletteStandard = GetTemplateChild(nameof(PART_ColorPaletteStandard)) as ColorPalette;
    PART_ColorPaletteAvailable = GetTemplateChild(nameof(PART_ColorPaletteAvailable)) as ColorPalette;
    PART_ColorPaletteCustom01 = GetTemplateChild(nameof(PART_ColorPaletteCustom01)) as ColorPalette;
    PART_ColorPaletteCustom02 = GetTemplateChild(nameof(PART_ColorPaletteCustom02)) as ColorPalette;
    PART_ColorPaletteRecent = GetTemplateChild(nameof(PART_ColorPaletteRecent)) as ColorPalette;

    PART_PopupTabControl = GetTemplateChild(nameof(PART_PopupTabControl)) as TabControl;
    PART_ColorPalettesTab = GetTemplateChild(nameof(PART_ColorPalettesTab)) as TabItem;
    PART_AdvancedTab = GetTemplateChild(nameof(PART_AdvancedTab)) as TabItem;

    if (PART_ColorPaletteStandard != null)
    {
      PART_ColorPaletteStandard.SelectionChanged += ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteAvailable != null)
    {
      PART_ColorPaletteAvailable.SelectionChanged += ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteCustom01 != null)
    {
      PART_ColorPaletteCustom01.SelectionChanged += ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteCustom02 != null)
    {
      PART_ColorPaletteCustom02.SelectionChanged += ColorPalette_SelectionChanged;
    }

    if (PART_ColorPaletteRecent != null)
    {
      PART_ColorPaletteRecent.SelectionChanged += ColorPalette_SelectionChanged;
    }

    ValidateTabItems();
  }

  internal override void OnSelectedColorChanged(Color? oldValue, Color? newValue)
  {
    base.OnSelectedColorChanged(newValue, oldValue);

    PART_ColorPaletteAvailable?.SetCurrentValue(Selector.SelectedValueProperty, newValue);
    PART_ColorPaletteStandard?.SetCurrentValue(Selector.SelectedValueProperty, newValue);
    PART_ColorPaletteCustom01?.SetCurrentValue(Selector.SelectedValueProperty, newValue);
    PART_ColorPaletteCustom02?.SetCurrentValue(Selector.SelectedValueProperty, newValue);
    PART_ColorPaletteRecent?.SetCurrentValue(Selector.SelectedValueProperty, newValue);

    if (AddToRecentColorsTrigger == AddToRecentColorsTrigger.SelectedColorChanged && SelectedColor.HasValue)
    {
      BuildInColorPalettes.AddColorToRecentColors(newValue, RecentColorPaletteItemsSource, BuildInColorPalettes.GetMaximumRecentColorsCount(this));
    }
  }

  private void ColorPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (sender is ColorPalette colorPalette && !ColorIsUpdating)
    {
      SetCurrentValue(SelectedColorProperty, colorPalette.SelectedItem as Color?);

      if (CloseOnSelectedColorChanged)
      {
        Close();
      }
    }
  }

  private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is not ColorPicker colorPicker)
    {
      return;
    }

    if ((bool)e.NewValue)
    {
      colorPicker.RaiseEvent(new RoutedEventArgs(DropDownOpenedEvent));

      var action = new Action(() =>
      {
        colorPicker.Focus();

        Mouse.Capture(colorPicker, CaptureMode.SubTree);

        colorPicker.ValidateTabItems();

        if (colorPicker.PART_PopupTabControl is not null)
        {
          if (ReferenceEquals(colorPicker.PART_PopupTabControl.SelectedItem, colorPicker.PART_ColorPalettesTab))
          {
            if (colorPicker.IsStandardColorPaletteVisible && colorPicker.PART_ColorPaletteStandard != null)
            {
              colorPicker.PART_ColorPaletteStandard.FocusSelectedItem();
            }
            else if (colorPicker.IsAvailableColorPaletteVisible && colorPicker.PART_ColorPaletteAvailable != null)
            {
              colorPicker.PART_ColorPaletteAvailable.FocusSelectedItem();
            }
            else if (colorPicker.IsCustomColorPalette01Visible && colorPicker.PART_ColorPaletteCustom01 != null)
            {
              colorPicker.PART_ColorPaletteCustom01.FocusSelectedItem();
            }
            else if (colorPicker.IsCustomColorPalette02Visible && colorPicker.PART_ColorPaletteCustom02 != null)
            {
              colorPicker.PART_ColorPaletteCustom02.FocusSelectedItem();
            }
            else if (colorPicker.IsRecentColorPaletteVisible && colorPicker.PART_ColorPaletteRecent != null)
            {
              colorPicker.PART_ColorPaletteRecent.FocusSelectedItem();
            }
          }
          else if (ReferenceEquals(colorPicker.PART_PopupTabControl.SelectedItem, colorPicker.PART_AdvancedTab))
          {
            colorPicker.PART_AdvancedTab.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
          }
        }
      });

      colorPicker.Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
    }
    else
    {
      colorPicker.RaiseEvent(new RoutedEventArgs(DropDownClosedEvent));

      if (ReferenceEquals(Mouse.Captured, colorPicker))
      {
        Mouse.Capture(null);
      }

      if (colorPicker.AddToRecentColorsTrigger == AddToRecentColorsTrigger.ColorPickerClosed && colorPicker.SelectedColor.HasValue)
      {
        BuildInColorPalettes.AddColorToRecentColors(colorPicker.SelectedColor, colorPicker.RecentColorPaletteItemsSource, BuildInColorPalettes.GetMaximumRecentColorsCount(colorPicker));
      }
    }
  }

  private static void OnLostMouseCapture(object sender, MouseEventArgs e)
  {
    ColorPicker colorPicker = (ColorPicker)sender;

    if (!ReferenceEquals(Mouse.Captured, colorPicker))
    {
      if (ReferenceEquals(e.OriginalSource, colorPicker))
      {
        if (Mouse.Captured is null || !(Mouse.Captured as DependencyObject)?.IsDescendantOf(colorPicker) == true)
        {
          colorPicker.Close();
        }
      }
      else
      {
        if ((e.OriginalSource as DependencyObject)?.IsDescendantOf(colorPicker) == true)
        {
          // Take capture if one of our children gave up capture (by closing their drop down)
          if (colorPicker.IsDropDownOpen && Mouse.Captured is null)
          {
            Mouse.Capture(colorPicker, CaptureMode.SubTree);
            e.Handled = true;
          }
        }
        else
        {
          colorPicker.Close();
        }
      }
    }
  }

  private static void OnMouseDown(object sender, MouseButtonEventArgs e)
  {
    ColorPicker colorPicker = (ColorPicker)sender;

    // If we (or one of our children) are clicked, claim the focus (don't steal focus if our context menu is clicked)
    if ((!colorPicker.ContextMenu?.IsOpen ?? true) && !colorPicker.IsKeyboardFocusWithin)
    {
      colorPicker.Focus();
    }

    e.Handled = true; // Always handle so that parents won't take focus away

    if (ReferenceEquals(Mouse.Captured, colorPicker) && ReferenceEquals(e.OriginalSource, colorPicker))
    {
      colorPicker.Close();
    }
  }

  private void Close()
  {
    if (IsDropDownOpen)
    {
      SetCurrentValue(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
    }
  }

  private static void OnIsTabVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is ColorPicker colorPicker && colorPicker.IsInitialized)
    {
      colorPicker.ValidateTabItems();
    }
  }

  private void ValidateTabItems()
  {
    if (PART_PopupTabControl is null
        || PART_ColorPalettesTab is null
        || PART_AdvancedTab is null)
    {
      return;
    }

    if (!IsAdvancedTabVisible && !IsColorPalettesTabVisible)
    {
      // If both TabItems are invisible we set the content to null
      PART_PopupTabControl.SelectedIndex = -1;
    }
    else if (ReferenceEquals(PART_PopupTabControl.SelectedItem, PART_ColorPalettesTab) && !IsColorPalettesTabVisible)
    {
      // If the color palettes tab is selected but not visible we select the advanced tab and vice vera
      PART_PopupTabControl.SelectedItem = PART_AdvancedTab;
    }
    else if (ReferenceEquals(PART_PopupTabControl.SelectedItem, PART_AdvancedTab) && !IsAdvancedTabVisible)
    {
      PART_PopupTabControl.SelectedItem = PART_ColorPalettesTab;
    }
    else
    {
      if (IsColorPalettesTabVisible)
      {
        PART_ColorPalettesTab.IsSelected = true;
      }
      else if (IsAdvancedTabVisible)
      {
        PART_AdvancedTab.IsSelected = true;
      }
    }
  }

  static ColorPicker()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
    EventManager.RegisterClassHandler(typeof(ColorPicker), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
    EventManager.RegisterClassHandler(typeof(ColorPicker), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseDown), true); // call us even if the transparent button in the style gets the click.
  }
}