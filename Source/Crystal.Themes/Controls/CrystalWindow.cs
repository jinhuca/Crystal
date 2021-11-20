// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Shapes;
using Crystal.Themes.Automation.Peers;
using Crystal.Themes.Behaviors;
using Crystal.Themes.Controls.Dialogs;
using Crystal.Themes.ValueBoxes;
using Crystal.Themes.Theming;
using Crystal.Behaviors;
using Crystal.Themes.Native;
using Crystal.Themes.Standard;

namespace Crystal.Themes.Controls
{
  [TemplatePart(Name = PART_Icon, Type = typeof(UIElement))]
  [TemplatePart(Name = PART_TitleBar, Type = typeof(UIElement))]
  [TemplatePart(Name = PART_WindowTitleBackground, Type = typeof(UIElement))]
  [TemplatePart(Name = PART_WindowTitleThumb, Type = typeof(Thumb))]
  [TemplatePart(Name = PART_FlyoutModalDragMoveThumb, Type = typeof(Thumb))]
  [TemplatePart(Name = PART_LeftWindowCommands, Type = typeof(ContentPresenter))]
  [TemplatePart(Name = PART_RightWindowCommands, Type = typeof(ContentPresenter))]
  [TemplatePart(Name = PART_WindowButtonCommands, Type = typeof(ContentPresenter))]
  [TemplatePart(Name = PART_OverlayBox, Type = typeof(Grid))]
  [TemplatePart(Name = PART_CrystalActiveDialogContainer, Type = typeof(Grid))]
  [TemplatePart(Name = PART_CrystalInactiveDialogsContainer, Type = typeof(Grid))]
  [TemplatePart(Name = PART_FlyoutModal, Type = typeof(Rectangle))]
  [TemplatePart(Name = PART_Content, Type = typeof(CrystalContentControl))]
  public class CrystalWindow : Window
  {
    private const string PART_Icon = "PART_Icon";
    private const string PART_TitleBar = "PART_TitleBar";
    private const string PART_WindowTitleBackground = "PART_WindowTitleBackground";
    private const string PART_WindowTitleThumb = "PART_WindowTitleThumb";
    private const string PART_FlyoutModalDragMoveThumb = "PART_FlyoutModalDragMoveThumb";
    private const string PART_LeftWindowCommands = "PART_LeftWindowCommands";
    private const string PART_RightWindowCommands = "PART_RightWindowCommands";
    private const string PART_WindowButtonCommands = "PART_WindowButtonCommands";
    private const string PART_OverlayBox = "PART_OverlayBox";
    private const string PART_CrystalActiveDialogContainer = "PART_CrystalActiveDialogContainer";
    private const string PART_CrystalInactiveDialogsContainer = "PART_CrystalInactiveDialogsContainer";
    private const string PART_FlyoutModal = "PART_FlyoutModal";
    private const string PART_Content = "PART_Content";

    FrameworkElement? icon;
    UIElement? titleBar;
    UIElement? titleBarBackground;
    Thumb? windowTitleThumb;
    Thumb? flyoutModalDragMoveThumb;
    private IInputElement? restoreFocus;
    internal ContentPresenter? LeftWindowCommandsPresenter;
    internal ContentPresenter? RightWindowCommandsPresenter;
    internal ContentPresenter? WindowButtonCommandsPresenter;

    internal Grid? overlayBox;
    internal Grid? crystalActiveDialogContainer;
    internal Grid? crystalInactiveDialogContainer;
    private Storyboard? overlayStoryboard;
    Rectangle? flyoutModal;

    private EventHandler? onOverlayFadeInStoryboardCompleted = null;
    private EventHandler? onOverlayFadeOutStoryboardCompleted = null;

    public static readonly DependencyProperty ShowIconOnTitleBarProperty = DependencyProperty.Register(
      nameof(ShowIconOnTitleBar),
      typeof(bool),
      typeof(CrystalWindow),
      new PropertyMetadata(BooleanBoxes.TrueBox, OnShowIconOnTitleBarPropertyChangedCallback));

    private static void OnShowIconOnTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var window = (CrystalWindow)d;
      if (e.NewValue != e.OldValue)
      {
        window.UpdateIconVisibility();
      }
    }

    /// <summary>
    /// Get or sets whether the TitleBar icon is visible or not.
    /// </summary>
    public bool ShowIconOnTitleBar
    {
      get => (bool)GetValue(ShowIconOnTitleBarProperty);
      set => SetValue(ShowIconOnTitleBarProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty IconEdgeModeProperty = DependencyProperty.Register(
      nameof(IconEdgeMode),
      typeof(EdgeMode),
      typeof(CrystalWindow),
      new PropertyMetadata(EdgeMode.Aliased));

    public EdgeMode IconEdgeMode
    {
      get => (EdgeMode)GetValue(IconEdgeModeProperty);
      set => SetValue(IconEdgeModeProperty, value);
    }

    public static readonly DependencyProperty IconBitmapScalingModeProperty = DependencyProperty.Register(
      nameof(IconBitmapScalingMode),
      typeof(BitmapScalingMode),
      typeof(CrystalWindow),
      new PropertyMetadata(BitmapScalingMode.HighQuality));

    public BitmapScalingMode IconBitmapScalingMode
    {
      get => (BitmapScalingMode)GetValue(IconBitmapScalingModeProperty);
      set => SetValue(IconBitmapScalingModeProperty, value);
    }

    public static readonly DependencyProperty IconScalingModeProperty = DependencyProperty.Register(
      nameof(IconScalingMode),
      typeof(MultiFrameImageMode),
      typeof(CrystalWindow),
      new FrameworkPropertyMetadata(MultiFrameImageMode.ScaleDownLargerFrame, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Gets or sets the scaling mode for the TitleBar icon.
    /// </summary>
    public MultiFrameImageMode IconScalingMode
    {
      get => (MultiFrameImageMode)GetValue(IconScalingModeProperty);
      set => SetValue(IconScalingModeProperty, value);
    }

    /// <summary>Identifies the <see cref="ShowTitleBar"/> dependency property.</summary>
    public static readonly DependencyProperty ShowTitleBarProperty
        = DependencyProperty.Register(nameof(ShowTitleBar),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox, OnShowTitleBarPropertyChangedCallback, OnShowTitleBarCoerceValueCallback));

    [MustUseReturnValue]
    private static object? OnShowTitleBarCoerceValueCallback(DependencyObject d, object? value)
    {
      // if UseNoneWindowStyle = true no title bar should be shown
      return ((CrystalWindow)d).UseNoneWindowStyle ? false : value;
    }

    /// <summary>
    /// Gets or sets whether the TitleBar is visible or not.
    /// </summary>
    public bool ShowTitleBar
    {
      get => (bool)GetValue(ShowTitleBarProperty);
      set => SetValue(ShowTitleBarProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowDialogsOverTitleBar"/> dependency property.</summary>
    public static readonly DependencyProperty ShowDialogsOverTitleBarProperty
        = DependencyProperty.Register(nameof(ShowDialogsOverTitleBar),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Get or sets whether a dialog will be shown over the TitleBar.
    /// </summary>
    public bool ShowDialogsOverTitleBar
    {
      get => (bool)GetValue(ShowDialogsOverTitleBarProperty);
      set => SetValue(ShowDialogsOverTitleBarProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsAnyDialogOpen"/> dependency property.</summary>
    internal static readonly DependencyPropertyKey IsAnyDialogOpenPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(IsAnyDialogOpen),
                                              typeof(bool),
                                              typeof(CrystalWindow),
                                              new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsAnyDialogOpen"/> dependency property.</summary>
    public static readonly DependencyProperty IsAnyDialogOpenProperty = IsAnyDialogOpenPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets whether that there are one or more dialogs open.
    /// </summary>
    public bool IsAnyDialogOpen
    {
      get => (bool)GetValue(IsAnyDialogOpenProperty);
      protected set => SetValue(IsAnyDialogOpenPropertyKey, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowMinButton"/> dependency property.</summary>
    public static readonly DependencyProperty ShowMinButtonProperty
        = DependencyProperty.Register(nameof(ShowMinButton),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether if the minimize button is visible and the minimize system menu is enabled.
    /// </summary>
    public bool ShowMinButton
    {
      get => (bool)GetValue(ShowMinButtonProperty);
      set => SetValue(ShowMinButtonProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowMaxRestoreButton"/> dependency property.</summary>
    public static readonly DependencyProperty ShowMaxRestoreButtonProperty
        = DependencyProperty.Register(nameof(ShowMaxRestoreButton),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether if the maximize/restore button is visible and the maximize/restore system menu is enabled.
    /// </summary>
    public bool ShowMaxRestoreButton
    {
      get => (bool)GetValue(ShowMaxRestoreButtonProperty);
      set => SetValue(ShowMaxRestoreButtonProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowCloseButton"/> dependency property.</summary>
    public static readonly DependencyProperty ShowCloseButtonProperty
        = DependencyProperty.Register(nameof(ShowCloseButton),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether if the close button is visible.
    /// </summary>
    public bool ShowCloseButton
    {
      get => (bool)GetValue(ShowCloseButtonProperty);
      set => SetValue(ShowCloseButtonProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsMinButtonEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsMinButtonEnabledProperty
        = DependencyProperty.Register(nameof(IsMinButtonEnabled),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets if the minimize button is enabled.
    /// </summary>
    public bool IsMinButtonEnabled
    {
      get => (bool)GetValue(IsMinButtonEnabledProperty);
      set => SetValue(IsMinButtonEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsMaxRestoreButtonEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsMaxRestoreButtonEnabledProperty
        = DependencyProperty.Register(nameof(IsMaxRestoreButtonEnabled),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets if the maximize/restore button is enabled.
    /// </summary>
    public bool IsMaxRestoreButtonEnabled
    {
      get => (bool)GetValue(IsMaxRestoreButtonEnabledProperty);
      set => SetValue(IsMaxRestoreButtonEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsCloseButtonEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsCloseButtonEnabledProperty
        = DependencyProperty.Register(nameof(IsCloseButtonEnabled),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets if the close button is enabled.
    /// </summary>
    public bool IsCloseButtonEnabled
    {
      get => (bool)GetValue(IsCloseButtonEnabledProperty);
      set => SetValue(IsCloseButtonEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsCloseButtonEnabledWithDialog"/> dependency property.</summary>
    internal static readonly DependencyPropertyKey IsCloseButtonEnabledWithDialogPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(IsCloseButtonEnabledWithDialog),
                                              typeof(bool),
                                              typeof(CrystalWindow),
                                              new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>Identifies the <see cref="IsCloseButtonEnabledWithDialog"/> dependency property.</summary>
    public static readonly DependencyProperty IsCloseButtonEnabledWithDialogProperty = IsCloseButtonEnabledWithDialogPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets whether if the close button is enabled if a dialog is open.
    /// It's true if <see cref="ShowDialogsOverTitleBar"/> or the <see cref="CrystalDialogSettings.OwnerCanCloseWithDialog"/> is set to true
    /// otherwise false.
    /// </summary>
    public bool IsCloseButtonEnabledWithDialog
    {
      get => (bool)GetValue(IsCloseButtonEnabledWithDialogProperty);
      protected set => SetValue(IsCloseButtonEnabledWithDialogPropertyKey, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowSystemMenu"/> dependency property.</summary>
    public static readonly DependencyProperty ShowSystemMenuProperty
        = DependencyProperty.Register(nameof(ShowSystemMenu),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets a value that indicates whether the system menu should popup with left mouse click on the window icon.
    /// </summary>
    public bool ShowSystemMenu
    {
      get => (bool)GetValue(ShowSystemMenuProperty);
      set => SetValue(ShowSystemMenuProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ShowSystemMenuOnRightClick"/> dependency property.</summary>
    public static readonly DependencyProperty ShowSystemMenuOnRightClickProperty
        = DependencyProperty.Register(nameof(ShowSystemMenuOnRightClick),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets a value that indicates whether the system menu should popup with right mouse click if the mouse position is on title bar or on the entire window if it has no TitleBar (and no TitleBar height).
    /// </summary>
    public bool ShowSystemMenuOnRightClick
    {
      get => (bool)GetValue(ShowSystemMenuOnRightClickProperty);
      set => SetValue(ShowSystemMenuOnRightClickProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="TitleBarHeight"/> dependency property.</summary>
    public static readonly DependencyProperty TitleBarHeightProperty
        = DependencyProperty.Register(nameof(TitleBarHeight),
                                      typeof(int),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(30, TitleBarHeightPropertyChangedCallback));

    private static void TitleBarHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        ((CrystalWindow)d).UpdateTitleBarElementsVisibility();
      }
    }

    /// <summary>
    /// Gets or sets the TitleBar's height.
    /// </summary>
    public int TitleBarHeight
    {
      get => (int)GetValue(TitleBarHeightProperty);
      set => SetValue(TitleBarHeightProperty, value);
    }

    /// <summary>Identifies the <see cref="TitleCharacterCasing"/> dependency property.</summary>
    public static readonly DependencyProperty TitleCharacterCasingProperty
        = DependencyProperty.Register(nameof(TitleCharacterCasing),
                                      typeof(CharacterCasing),
                                      typeof(CrystalWindow),
                                      new FrameworkPropertyMetadata(CharacterCasing.Upper, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure),
                                      value => CharacterCasing.Normal <= (CharacterCasing)value && (CharacterCasing)value <= CharacterCasing.Upper);

    /// <summary>
    /// Gets or sets the Character casing of the title.
    /// </summary>
    public CharacterCasing TitleCharacterCasing
    {
      get => (CharacterCasing)GetValue(TitleCharacterCasingProperty);
      set => SetValue(TitleCharacterCasingProperty, value);
    }

    /// <summary>Identifies the <see cref="TitleAlignment"/> dependency property.</summary>
    public static readonly DependencyProperty TitleAlignmentProperty
        = DependencyProperty.Register(nameof(TitleAlignment),
                                      typeof(HorizontalAlignment),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(HorizontalAlignment.Stretch, OnTitleAlignmentChanged));

    private static void OnTitleAlignmentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue)
      {
        var window = (CrystalWindow)dependencyObject;

        window.SizeChanged -= window.CrystalWindow_SizeChanged;
        if (e.NewValue is HorizontalAlignment horizontalAlignment && horizontalAlignment == HorizontalAlignment.Center && window.titleBar != null)
        {
          window.SizeChanged += window.CrystalWindow_SizeChanged;
        }
      }
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the title.
    /// </summary>
    public HorizontalAlignment TitleAlignment
    {
      get => (HorizontalAlignment)GetValue(TitleAlignmentProperty);
      set => SetValue(TitleAlignmentProperty, value);
    }

    /// <summary>Identifies the <see cref="SaveWindowPosition"/> dependency property.</summary>
    public static readonly DependencyProperty SaveWindowPositionProperty
        = DependencyProperty.Register(nameof(SaveWindowPosition),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets whether the window will save it's position and size.
    /// </summary>
    public bool SaveWindowPosition
    {
      get => (bool)GetValue(SaveWindowPositionProperty);
      set => SetValue(SaveWindowPositionProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="WindowPlacementSettings"/> dependency property.</summary>
    public static readonly DependencyProperty WindowPlacementSettingsProperty
        = DependencyProperty.Register(nameof(WindowPlacementSettings),
                                      typeof(IWindowPlacementSettings),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    ///  Gets or sets the settings to save and load the position and size of the window.
    /// </summary>
    public IWindowPlacementSettings? WindowPlacementSettings
    {
      get => (IWindowPlacementSettings?)GetValue(WindowPlacementSettingsProperty);
      set => SetValue(WindowPlacementSettingsProperty, value);
    }

    /// <summary>Identifies the <see cref="TitleForeground"/> dependency property.</summary>
    public static readonly DependencyProperty TitleForegroundProperty
        = DependencyProperty.Register(nameof(TitleForeground),
                                      typeof(Brush),
                                      typeof(CrystalWindow));

    /// <summary>
    /// Gets or sets the brush used for the TitleBar's foreground.
    /// </summary>
    public Brush? TitleForeground
    {
      get => (Brush?)GetValue(TitleForegroundProperty);
      set => SetValue(TitleForegroundProperty, value);
    }

    /// <summary>Identifies the <see cref="TitleTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty TitleTemplateProperty
        = DependencyProperty.Register(nameof(TitleTemplate),
                                      typeof(DataTemplate),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the <see cref="DataTemplate"/> for the <see cref="Window.Title"/>.
    /// </summary>
    public DataTemplate? TitleTemplate
    {
      get => (DataTemplate?)GetValue(TitleTemplateProperty);
      set => SetValue(TitleTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="WindowTitleBrush"/> dependency property.</summary>
    public static readonly DependencyProperty WindowTitleBrushProperty
        = DependencyProperty.Register(nameof(WindowTitleBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(Brushes.Transparent));

    /// <summary>
    /// Gets or sets the brush used for the background of the TitleBar.
    /// </summary>
    public Brush WindowTitleBrush
    {
      get => (Brush)GetValue(WindowTitleBrushProperty);
      set => SetValue(WindowTitleBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="NonActiveWindowTitleBrush"/> dependency property.</summary>
    public static readonly DependencyProperty NonActiveWindowTitleBrushProperty
        = DependencyProperty.Register(nameof(NonActiveWindowTitleBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(Brushes.Gray));

    /// <summary>
    /// Gets or sets the non-active brush used for the background of the TitleBar.
    /// </summary>
    public Brush NonActiveWindowTitleBrush
    {
      get => (Brush)GetValue(NonActiveWindowTitleBrushProperty);
      set => SetValue(NonActiveWindowTitleBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="NonActiveBorderBrush"/> dependency property.</summary>
    public static readonly DependencyProperty NonActiveBorderBrushProperty
        = DependencyProperty.Register(nameof(NonActiveBorderBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(Brushes.Gray));

    /// <summary>
    /// Gets or sets the non-active brush used for the border of the window.
    /// </summary>
    public Brush NonActiveBorderBrush
    {
      get => (Brush)GetValue(NonActiveBorderBrushProperty);
      set => SetValue(NonActiveBorderBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="GlowBrush"/> dependency property.</summary>
    public static readonly DependencyProperty GlowBrushProperty
        = DependencyProperty.Register(nameof(GlowBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the brush used for glow border of the window.
    /// </summary>
    public Brush? GlowBrush
    {
      get => (Brush?)GetValue(GlowBrushProperty);
      set => SetValue(GlowBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="NonActiveGlowBrush"/> dependency property.</summary>
    public static readonly DependencyProperty NonActiveGlowBrushProperty
        = DependencyProperty.Register(nameof(NonActiveGlowBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the non-active brush used for glow border of the window.
    /// </summary>
    public Brush? NonActiveGlowBrush
    {
      get => (Brush?)GetValue(NonActiveGlowBrushProperty);
      set => SetValue(NonActiveGlowBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="OverlayBrush"/> dependency property.</summary>
    public static readonly DependencyProperty OverlayBrushProperty
        = DependencyProperty.Register(nameof(OverlayBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the brush used for the overlay when a dialog is open.
    /// </summary>
    public Brush? OverlayBrush
    {
      get => (Brush?)GetValue(OverlayBrushProperty);
      set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="OverlayOpacity"/> dependency property.</summary>
    public static readonly DependencyProperty OverlayOpacityProperty
        = DependencyProperty.Register(nameof(OverlayOpacity),
                                      typeof(double),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(0.7d));

    /// <summary>
    /// Gets or sets the opacity used for the overlay when a dialog is open.
    /// </summary>
    public double OverlayOpacity
    {
      get => (double)GetValue(OverlayOpacityProperty);
      set => SetValue(OverlayOpacityProperty, value);
    }

    /// <summary>Identifies the <see cref="FlyoutOverlayBrush"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutOverlayBrushProperty
        = DependencyProperty.Register(nameof(FlyoutOverlayBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the brush used for the overlay when a modal Flyout is open.
    /// </summary>
    public Brush? FlyoutOverlayBrush
    {
      get => (Brush?)GetValue(FlyoutOverlayBrushProperty);
      set => SetValue(FlyoutOverlayBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="OverlayFadeIn"/> dependency property.</summary>
    public static readonly DependencyProperty OverlayFadeInProperty
        = DependencyProperty.Register(nameof(OverlayFadeIn),
                                      typeof(Storyboard),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(default(Storyboard)));

    /// <summary>
    /// Gets or sets the storyboard for the overlay fade in effect.
    /// </summary>
    public Storyboard? OverlayFadeIn
    {
      get => (Storyboard?)GetValue(OverlayFadeInProperty);
      set => SetValue(OverlayFadeInProperty, value);
    }

    /// <summary>Identifies the <see cref="OverlayFadeOut"/> dependency property.</summary>
    public static readonly DependencyProperty OverlayFadeOutProperty
        = DependencyProperty.Register(nameof(OverlayFadeOut),
                                      typeof(Storyboard),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(default(Storyboard)));

    /// <summary>
    /// Gets or sets the storyboard for the overlay fade out effect.
    /// </summary>
    public Storyboard? OverlayFadeOut
    {
      get => (Storyboard?)GetValue(OverlayFadeOutProperty);
      set => SetValue(OverlayFadeOutProperty, value);
    }

    /// <summary>Identifies the <see cref="Flyouts"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutsProperty
        = DependencyProperty.Register(nameof(Flyouts),
                                      typeof(FlyoutsControl),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null, UpdateLogicalChildren));

    /// <summary>
    /// Gets or sets a <see cref="FlyoutsControl"/> host for the <see cref="Flyout"/> controls.
    /// </summary>
    public FlyoutsControl? Flyouts
    {
      get => (FlyoutsControl?)GetValue(FlyoutsProperty);
      set => SetValue(FlyoutsProperty, value);
    }

    /// <summary>Identifies the <see cref="WindowTransitionsEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty WindowTransitionsEnabledProperty
        = DependencyProperty.Register(nameof(WindowTransitionsEnabled),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether the start animation of the window content is available.
    /// </summary>
    public bool WindowTransitionsEnabled
    {
      get => (bool)GetValue(WindowTransitionsEnabledProperty);
      set => SetValue(WindowTransitionsEnabledProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="CrystalDialogOptions"/> dependency property.</summary>
    public static readonly DependencyProperty CrystalDialogOptionsProperty
        = DependencyProperty.Register(nameof(CrystalDialogOptions),
                                      typeof(CrystalDialogSettings),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(default(CrystalDialogSettings)));

    /// <summary>
    /// Gets or sets the default settings for the dialogs.
    /// </summary>
    public CrystalDialogSettings? CrystalDialogOptions
    {
      get => (CrystalDialogSettings?)GetValue(CrystalDialogOptionsProperty);
      set => SetValue(CrystalDialogOptionsProperty, value);
    }

    /// <summary>Identifies the <see cref="IconTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty IconTemplateProperty
        = DependencyProperty.Register(nameof(IconTemplate),
                                      typeof(DataTemplate),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the <see cref="DataTemplate"/> for the icon on the TitleBar.
    /// </summary>
    public DataTemplate? IconTemplate
    {
      get => (DataTemplate?)GetValue(IconTemplateProperty);
      set => SetValue(IconTemplateProperty, value);
    }

    /// <summary>Identifies the <see cref="LeftWindowCommands"/> dependency property.</summary>
    public static readonly DependencyProperty LeftWindowCommandsProperty
        = DependencyProperty.Register(nameof(LeftWindowCommands),
                                      typeof(WindowCommands),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null, OnLeftWindowCommandsPropertyChanged));

    private static void OnLeftWindowCommandsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue is WindowCommands windowCommands)
      {
        AutomationProperties.SetName(windowCommands, nameof(LeftWindowCommands));
      }

      UpdateLogicalChildren(d, e);
    }

    /// <summary>
    /// Gets or sets the <see cref="WindowCommands"/> host on the left side of the TitleBar.
    /// </summary>
    public WindowCommands? LeftWindowCommands
    {
      get => (WindowCommands?)GetValue(LeftWindowCommandsProperty);
      set => SetValue(LeftWindowCommandsProperty, value);
    }

    /// <summary>Identifies the <see cref="RightWindowCommands"/> dependency property.</summary>
    public static readonly DependencyProperty RightWindowCommandsProperty
        = DependencyProperty.Register(nameof(RightWindowCommands),
                                      typeof(WindowCommands),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null, OnRightWindowCommandsPropertyChanged));

    private static void OnRightWindowCommandsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue is WindowCommands windowCommands)
      {
        AutomationProperties.SetName(windowCommands, nameof(RightWindowCommands));
      }

      UpdateLogicalChildren(d, e);
    }

    /// <summary>
    /// Gets or sets the <see cref="WindowCommands"/> host on the right side of the TitleBar.
    /// </summary>
    public WindowCommands? RightWindowCommands
    {
      get => (WindowCommands?)GetValue(RightWindowCommandsProperty);
      set => SetValue(RightWindowCommandsProperty, value);
    }

    /// <summary>Identifies the <see cref="WindowButtonCommands"/> dependency property.</summary>
    public static readonly DependencyProperty WindowButtonCommandsProperty
        = DependencyProperty.Register(nameof(WindowButtonCommands),
                                      typeof(WindowButtonCommands),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(null, UpdateLogicalChildren));

    /// <summary>
    /// Gets or sets the <see cref="WindowButtonCommands"/> host that shows the minimize/maximize/restore/close buttons.
    /// </summary>
    public WindowButtonCommands? WindowButtonCommands
    {
      get => (WindowButtonCommands?)GetValue(WindowButtonCommandsProperty);
      set => SetValue(WindowButtonCommandsProperty, value);
    }

    /// <summary>Identifies the <see cref="LeftWindowCommandsOverlayBehavior"/> dependency property.</summary>
    public static readonly DependencyProperty LeftWindowCommandsOverlayBehaviorProperty
        = DependencyProperty.Register(nameof(LeftWindowCommandsOverlayBehavior),
                                      typeof(WindowCommandsOverlayBehavior),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(WindowCommandsOverlayBehavior.Never, OnShowTitleBarPropertyChangedCallback));

    private static void OnShowTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        ((CrystalWindow)d).UpdateTitleBarElementsVisibility();
      }
    }

    /// <summary>
    /// Gets or sets the overlay behavior for the <see cref="WindowCommands"/> host on the left side.
    /// </summary>
    public WindowCommandsOverlayBehavior LeftWindowCommandsOverlayBehavior
    {
      get => (WindowCommandsOverlayBehavior)GetValue(LeftWindowCommandsOverlayBehaviorProperty);
      set => SetValue(LeftWindowCommandsOverlayBehaviorProperty, value);
    }

    /// <summary>Identifies the <see cref="RightWindowCommandsOverlayBehavior"/> dependency property.</summary>
    public static readonly DependencyProperty RightWindowCommandsOverlayBehaviorProperty
        = DependencyProperty.Register(nameof(RightWindowCommandsOverlayBehavior),
                                      typeof(WindowCommandsOverlayBehavior),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(WindowCommandsOverlayBehavior.Never, OnShowTitleBarPropertyChangedCallback));

    /// <summary>
    /// Gets or sets the overlay behavior for the <see cref="WindowCommands"/> host on the right side.
    /// </summary>
    public WindowCommandsOverlayBehavior RightWindowCommandsOverlayBehavior
    {
      get => (WindowCommandsOverlayBehavior)GetValue(RightWindowCommandsOverlayBehaviorProperty);
      set => SetValue(RightWindowCommandsOverlayBehaviorProperty, value);
    }

    /// <summary>Identifies the <see cref="WindowButtonCommandsOverlayBehavior"/> dependency property.</summary>
    public static readonly DependencyProperty WindowButtonCommandsOverlayBehaviorProperty
        = DependencyProperty.Register(nameof(WindowButtonCommandsOverlayBehavior),
                                      typeof(OverlayBehavior),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(OverlayBehavior.Always, OnShowTitleBarPropertyChangedCallback));

    /// <summary>
    /// Gets or sets the overlay behavior for the <see cref="WindowButtonCommands"/> host.
    /// </summary>
    public OverlayBehavior WindowButtonCommandsOverlayBehavior
    {
      get => (OverlayBehavior)GetValue(WindowButtonCommandsOverlayBehaviorProperty);
      set => SetValue(WindowButtonCommandsOverlayBehaviorProperty, value);
    }

    /// <summary>Identifies the <see cref="IconOverlayBehavior"/> dependency property.</summary>
    public static readonly DependencyProperty IconOverlayBehaviorProperty
        = DependencyProperty.Register(nameof(IconOverlayBehavior),
                                      typeof(OverlayBehavior),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(OverlayBehavior.Never, OnShowTitleBarPropertyChangedCallback));

    /// <summary>
    /// Gets or sets the overlay behavior for the <see cref="Window.Icon"/>.
    /// </summary>
    public OverlayBehavior IconOverlayBehavior
    {
      get => (OverlayBehavior)GetValue(IconOverlayBehaviorProperty);
      set => SetValue(IconOverlayBehaviorProperty, value);
    }

    /// <summary>Identifies the <see cref="UseNoneWindowStyle"/> dependency property.</summary>
    public static readonly DependencyProperty UseNoneWindowStyleProperty
        = DependencyProperty.Register(nameof(UseNoneWindowStyle),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.FalseBox, OnUseNoneWindowStylePropertyChangedCallback));

    private static void OnUseNoneWindowStylePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        // if UseNoneWindowStyle = true no title bar should be shown
        // UseNoneWindowStyle means no title bar, window commands or min, max, close buttons
        ((CrystalWindow)d).CoerceValue(ShowTitleBarProperty);
      }
    }

    /// <summary>
    /// Gets or sets whether the window will force WindowStyle to None.
    /// </summary>
    public bool UseNoneWindowStyle
    {
      get => (bool)GetValue(UseNoneWindowStyleProperty);
      set => SetValue(UseNoneWindowStyleProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="OverrideDefaultWindowCommandsBrush"/> dependency property.</summary>
    public static readonly DependencyProperty OverrideDefaultWindowCommandsBrushProperty
        = DependencyProperty.Register(nameof(OverrideDefaultWindowCommandsBrush),
                                      typeof(Brush),
                                      typeof(CrystalWindow));

    /// <summary>
    /// Allows easy handling of <see cref="WindowCommands"/> brush. Theme is also applied based on this brush.
    /// </summary>
    public Brush? OverrideDefaultWindowCommandsBrush
    {
      get => (Brush?)GetValue(OverrideDefaultWindowCommandsBrushProperty);
      set => SetValue(OverrideDefaultWindowCommandsBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="IsWindowDraggable"/> dependency property.</summary>
    public static readonly DependencyProperty IsWindowDraggableProperty
        = DependencyProperty.Register(nameof(IsWindowDraggable),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether the whole window is draggable.
    /// </summary>
    public bool IsWindowDraggable
    {
      get => (bool)GetValue(IsWindowDraggableProperty);
      set => SetValue(IsWindowDraggableProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IgnoreTaskbarOnMaximize"/> dependency property.</summary>
    public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty
        = DependencyProperty.Register(nameof(IgnoreTaskbarOnMaximize),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets whether if the Taskbar should be ignored when maximizing the window.
    /// </summary>
    public bool IgnoreTaskbarOnMaximize
    {
      get => (bool)GetValue(IgnoreTaskbarOnMaximizeProperty);
      set => SetValue(IgnoreTaskbarOnMaximizeProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="ResizeBorderThickness"/> dependency property.</summary>
    public static readonly DependencyProperty ResizeBorderThicknessProperty
        = DependencyProperty.Register(nameof(ResizeBorderThickness),
                                      typeof(Thickness),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(new Thickness(6D)));

    /// <summary>
    /// Gets or sets resize border thickness of the window.
    /// </summary>
    public Thickness ResizeBorderThickness
    {
      get => (Thickness)GetValue(ResizeBorderThicknessProperty);
      set => SetValue(ResizeBorderThicknessProperty, value);
    }

    /// <summary>Identifies the <see cref="KeepBorderOnMaximize"/> dependency property.</summary>
    public static readonly DependencyProperty KeepBorderOnMaximizeProperty
        = DependencyProperty.Register(nameof(KeepBorderOnMaximize),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether if the border thickness should be kept on maximize
    /// if the MaxHeight/MaxWidth of the window is less than the monitor resolution.
    /// </summary>
    public bool KeepBorderOnMaximize
    {
      get => (bool)GetValue(KeepBorderOnMaximizeProperty);
      set => SetValue(KeepBorderOnMaximizeProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="TryToBeFlickerFree"/> dependency property.</summary>
    public static readonly DependencyProperty TryToBeFlickerFreeProperty
        = DependencyProperty.Register(nameof(TryToBeFlickerFree),
                                      typeof(bool),
                                      typeof(CrystalWindow),
                                      new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets whether the resizing of the window should be tried in a way that does not cause flicker/jitter, especially when resizing from the left side.
    /// </summary>
    /// <remarks>
    /// Please note that setting this to <c>true</c> may cause resize lag and black areas appearing on some systems.
    /// </remarks>
    public bool TryToBeFlickerFree
    {
      get => (bool)GetValue(TryToBeFlickerFreeProperty);
      set => SetValue(TryToBeFlickerFreeProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="FlyoutsStatusChanged"/> routed event.</summary>
    public static readonly RoutedEvent FlyoutsStatusChangedEvent
        = EventManager.RegisterRoutedEvent(nameof(FlyoutsStatusChanged),
                                           RoutingStrategy.Bubble,
                                           typeof(RoutedEventHandler),
                                           typeof(CrystalWindow));

    public event RoutedEventHandler FlyoutsStatusChanged
    {
      add => AddHandler(FlyoutsStatusChangedEvent, value);
      remove => RemoveHandler(FlyoutsStatusChangedEvent, value);
    }

    /// <summary>Identifies the <see cref="WindowTransitionCompleted"/> routed event.</summary>
    public static readonly RoutedEvent WindowTransitionCompletedEvent
        = EventManager.RegisterRoutedEvent(nameof(WindowTransitionCompleted),
                                           RoutingStrategy.Bubble,
                                           typeof(RoutedEventHandler),
                                           typeof(CrystalWindow));

    public event RoutedEventHandler WindowTransitionCompleted
    {
      add => AddHandler(WindowTransitionCompletedEvent, value);
      remove => RemoveHandler(WindowTransitionCompletedEvent, value);
    }

    /// <summary>
    /// Gets the window placement settings (can be overwritten).
    /// </summary>
    public virtual IWindowPlacementSettings? GetWindowPlacementSettings()
    {
      return WindowPlacementSettings ?? new WindowApplicationSettings(this);
    }

    private void UpdateIconVisibility()
    {
      var isVisible = (IconOverlayBehavior.HasFlag(OverlayBehavior.HiddenTitleBar) && !ShowTitleBar)
                      || (ShowIconOnTitleBar && ShowTitleBar);
      icon?.SetCurrentValue(VisibilityProperty, isVisible ? Visibility.Visible : Visibility.Collapsed);
    }

    private void UpdateTitleBarElementsVisibility()
    {
      UpdateIconVisibility();

      var newVisibility = TitleBarHeight > 0 && ShowTitleBar && !UseNoneWindowStyle ? Visibility.Visible : Visibility.Collapsed;

      titleBar?.SetCurrentValue(VisibilityProperty, newVisibility);
      titleBarBackground?.SetCurrentValue(VisibilityProperty, newVisibility);

      var leftWindowCommandsVisibility = LeftWindowCommandsOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) && !UseNoneWindowStyle ? Visibility.Visible : newVisibility;
      LeftWindowCommandsPresenter?.SetCurrentValue(VisibilityProperty, leftWindowCommandsVisibility);

      var rightWindowCommandsVisibility = RightWindowCommandsOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) && !UseNoneWindowStyle ? Visibility.Visible : newVisibility;
      RightWindowCommandsPresenter?.SetCurrentValue(VisibilityProperty, rightWindowCommandsVisibility);

      var windowButtonCommandsVisibility = WindowButtonCommandsOverlayBehavior.HasFlag(OverlayBehavior.HiddenTitleBar) ? Visibility.Visible : newVisibility;
      WindowButtonCommandsPresenter?.SetCurrentValue(VisibilityProperty, windowButtonCommandsVisibility);

      SetWindowEvents();
    }

    private bool CanUseOverlayFadingStoryboard([NotNullWhen(true)] Storyboard? sb, [NotNullWhen(true)] out DoubleAnimation? animation)
    {
      animation = null;

      if (sb is null)
      {
        return false;
      }

      sb.Dispatcher.VerifyAccess();

      animation = sb.Children.OfType<DoubleAnimation>().FirstOrDefault();

      if (animation is null)
      {
        return false;
      }

      return (sb.Duration.HasTimeSpan && sb.Duration.TimeSpan.Ticks > 0)
             || (sb.AccelerationRatio > 0)
             || (sb.DecelerationRatio > 0)
             || (animation.Duration.HasTimeSpan && animation.Duration.TimeSpan.Ticks > 0)
             || animation.AccelerationRatio > 0
             || animation.DecelerationRatio > 0;
    }

    /// <summary>
    /// Starts the overlay fade in effect.
    /// </summary>
    /// <returns>A task representing the process.</returns>
    public System.Threading.Tasks.Task ShowOverlayAsync()
    {
      if (overlayBox is null)
      {
        throw new InvalidOperationException("OverlayBox can not be founded in this CrystalWindow's template. Are you calling this before the window has loaded?");
      }

      var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

      if (IsOverlayVisible() && overlayStoryboard is null)
      {
        //No Task.FromResult in .NET 4.
        tcs.SetResult(null!);
        return tcs.Task;
      }

      Dispatcher.VerifyAccess();

      var sb = OverlayFadeIn?.Clone();
      overlayStoryboard = sb;
      if (CanUseOverlayFadingStoryboard(sb, out var animation))
      {
        overlayBox.SetCurrentValue(VisibilityProperty, Visibility.Visible);

        animation.To = OverlayOpacity;

        onOverlayFadeInStoryboardCompleted = (_, _) =>
            {
              sb.Completed -= onOverlayFadeInStoryboardCompleted;
              if (overlayStoryboard == sb)
              {
                overlayStoryboard = null;
              }

              tcs.TrySetResult(null!);
            };

        sb.Completed += onOverlayFadeInStoryboardCompleted;
        overlayBox.BeginStoryboard(sb);
      }
      else
      {
        ShowOverlay();
        tcs.TrySetResult(null!);
      }

      return tcs.Task;
    }

    /// <summary>
    /// Starts the overlay fade out effect.
    /// </summary>
    /// <returns>A task representing the process.</returns>
    public System.Threading.Tasks.Task HideOverlayAsync()
    {
      if (overlayBox is null)
      {
        throw new InvalidOperationException("OverlayBox can not be founded in this CrystalWindow's template. Are you calling this before the window has loaded?");
      }

      var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

      if (overlayBox.Visibility == Visibility.Visible && overlayBox.Opacity <= 0.0)
      {
        //No Task.FromResult in .NET 4.
        overlayBox.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
        tcs.SetResult(null!);
        return tcs.Task;
      }

      Dispatcher.VerifyAccess();

      var sb = OverlayFadeOut?.Clone();
      overlayStoryboard = sb;
      if (CanUseOverlayFadingStoryboard(sb, out var animation))
      {
        animation.To = 0d;

        onOverlayFadeOutStoryboardCompleted = (_, _) =>
            {
              sb.Completed -= onOverlayFadeOutStoryboardCompleted;
              if (overlayStoryboard == sb)
              {
                overlayBox.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
                overlayStoryboard = null;
              }

              tcs.TrySetResult(null!);
            };

        sb.Completed += onOverlayFadeOutStoryboardCompleted;
        overlayBox.BeginStoryboard(sb);
      }
      else
      {
        HideOverlay();
        tcs.TrySetResult(null!);
      }

      return tcs.Task;
    }

    public bool IsOverlayVisible()
    {
      if (overlayBox is null)
      {
        throw new InvalidOperationException("OverlayBox can not be founded in this CrystalWindow's template. Are you calling this before the window has loaded?");
      }

      return overlayBox.Visibility == Visibility.Visible && overlayBox.Opacity >= OverlayOpacity;
    }

    public void ShowOverlay()
    {
      overlayBox?.SetCurrentValue(VisibilityProperty, Visibility.Visible);
      overlayBox?.SetCurrentValue(OpacityProperty, OverlayOpacity);
    }

    public void HideOverlay()
    {
      overlayBox?.SetCurrentValue(OpacityProperty, 0d);
      overlayBox?.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
    }

    /// <summary>
    /// Stores the given element, or the last focused element via FocusManager, for restoring the focus after closing a dialog.
    /// </summary>
    /// <param name="thisElement">The element which will be focused again.</param>
    public void StoreFocus(IInputElement? thisElement = null)
    {
      Dispatcher.BeginInvoke(new Action(() => { restoreFocus = thisElement ?? (restoreFocus ?? FocusManager.GetFocusedElement(this)); }));
    }

    internal void RestoreFocus()
    {
      if (restoreFocus != null)
      {
        Dispatcher.BeginInvoke(new Action(() =>
            {
              Keyboard.Focus(restoreFocus);
              restoreFocus = null;
            }));
      }
    }

    /// <summary>
    /// Clears the stored element which would get the focus after closing a dialog.
    /// </summary>
    public void ResetStoredFocus()
    {
      restoreFocus = null;
    }

    static CrystalWindow()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(CrystalWindow), new FrameworkPropertyMetadata(typeof(CrystalWindow)));
    }

    /// <summary>
    /// Initializes a new instance of the Crystal.Themes.Controls.CrystalWindow class.
    /// </summary>
    public CrystalWindow()
    {
      SetCurrentValue(CrystalDialogOptionsProperty, new CrystalDialogSettings());

      // BorderlessWindowBehavior initialization has to occur in constructor. Otherwise the load event is fired early and performance of the window is degraded.
      InitializeWindowChromeBehavior();
      InitializeSettingsBehavior();
      InitializeGlowWindowBehavior();

      DataContextChanged += CrystalWindow_DataContextChanged;
      Loaded += CrystalWindow_Loaded;
    }

    private void CrystalWindow_Loaded(object sender, RoutedEventArgs e)
    {
      Flyouts ??= new FlyoutsControl();

      this.ResetAllWindowCommandsBrush();

      ThemeManager.Current.ThemeChanged += HandleThemeManagerThemeChanged;
      Unloaded += (_, _) => ThemeManager.Current.ThemeChanged -= HandleThemeManagerThemeChanged;
    }

    private void InitializeWindowChromeBehavior()
    {
      Interaction.GetBehaviors(this).Add(new BorderlessWindowBehavior());
    }

    private void InitializeGlowWindowBehavior()
    {
      var glowWindowBehavior = new GlowWindowBehavior();
      BindingOperations.SetBinding(glowWindowBehavior, GlowWindowBehavior.ResizeBorderThicknessProperty, new Binding { Path = new PropertyPath(ResizeBorderThicknessProperty), Source = this });
      BindingOperations.SetBinding(glowWindowBehavior, GlowWindowBehavior.GlowBrushProperty, new Binding { Path = new PropertyPath(GlowBrushProperty), Source = this });
      BindingOperations.SetBinding(glowWindowBehavior, GlowWindowBehavior.NonActiveGlowBrushProperty, new Binding { Path = new PropertyPath(NonActiveGlowBrushProperty), Source = this });
      Interaction.GetBehaviors(this).Add(glowWindowBehavior);
    }

    private void InitializeSettingsBehavior()
    {
      Interaction.GetBehaviors(this).Add(new WindowsSettingBehavior());
    }

    protected override async void OnClosing(CancelEventArgs e)
    {
      // Don't overwrite cancellation for close
      if (e.Cancel == false)
      {
        // #2409: don't close window if there is a dialog still open
        var dialog = await this.GetCurrentDialogAsync<CrystalDialogBase>();
        e.Cancel = dialog != null && (ShowDialogsOverTitleBar || !dialog.DialogSettings.OwnerCanCloseWithDialog);
      }

      base.OnClosing(e);
    }

    private void CrystalWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // Crystal add these controls to the window with AddLogicalChild method.
      // This has the side effect that the DataContext doesn't update, so do this now here.
      if (LeftWindowCommands != null)
      {
        LeftWindowCommands.DataContext = DataContext;
      }

      if (RightWindowCommands != null)
      {
        RightWindowCommands.DataContext = DataContext;
      }

      if (WindowButtonCommands != null)
      {
        WindowButtonCommands.DataContext = DataContext;
      }

      if (Flyouts != null)
      {
        Flyouts.DataContext = DataContext;
      }
    }

    private void CrystalWindow_SizeChanged(object sender, RoutedEventArgs e)
    {
      // this all works only for centered title
      if (TitleAlignment != HorizontalAlignment.Center
          || titleBar is null)
      {
        return;
      }

      // Half of this CrystalWindow
      var halfDistance = ActualWidth / 2;
      // Distance between center and left/right
      var margin = (Thickness)titleBar.GetValue(MarginProperty);
      var distanceToCenter = (titleBar.DesiredSize.Width - margin.Left - margin.Right) / 2;

      var iconWidth = icon?.ActualWidth ?? 0;
      var leftWindowCommandsWidth = LeftWindowCommands?.ActualWidth ?? 0;
      var rightWindowCommandsWidth = RightWindowCommands?.ActualWidth ?? 0;
      var windowButtonCommandsWith = WindowButtonCommands?.ActualWidth ?? 0;

      // Distance between right edge from LeftWindowCommands to left window side
      var distanceFromLeft = iconWidth + leftWindowCommandsWidth;
      // Distance between left edge from RightWindowCommands to right window side
      var distanceFromRight = rightWindowCommandsWidth + windowButtonCommandsWith;
      // Margin
      const double horizontalMargin = 5.0;

      var dLeft = distanceFromLeft + distanceToCenter + horizontalMargin;
      var dRight = distanceFromRight + distanceToCenter + horizontalMargin;
      if ((dLeft < halfDistance) && (dRight < halfDistance))
      {
        titleBar.SetCurrentValue(MarginProperty, default(Thickness));
        Grid.SetColumn(titleBar, 0);
        Grid.SetColumnSpan(titleBar, 5);
      }
      else
      {
        titleBar.SetCurrentValue(MarginProperty, new Thickness(leftWindowCommandsWidth, 0, rightWindowCommandsWidth, 0));
        Grid.SetColumn(titleBar, 2);
        Grid.SetColumnSpan(titleBar, 1);
      }
    }

    private void HandleThemeManagerThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
      this.Invoke(() =>
          {
            var flyouts = (Flyouts?.GetFlyouts().ToList() ?? new List<Flyout>());

            // since we disabled the ThemeManager OnThemeChanged part, we must change all children flyouts too
            // e.g if the FlyoutsControl is hosted in a UserControl
            var allChildFlyouts = (Content as DependencyObject)
                                  .FindChildren<FlyoutsControl>(true)
                                  .SelectMany(flyoutsControl => flyoutsControl.GetFlyouts());
            flyouts.AddRange(allChildFlyouts);

            if (!flyouts.Any())
            {
              // we must update the window command brushes!!!
              this.ResetAllWindowCommandsBrush();
              return;
            }

            var newTheme = ReferenceEquals(e.Target, this)
                      ? e.NewTheme
                      : ThemeManager.Current.DetectTheme(this);

            if (newTheme is null)
            {
              return;
            }

            foreach (var flyout in flyouts)
            {
              flyout.ChangeFlyoutTheme(newTheme);
            }

            this.HandleWindowCommandsForFlyouts(flyouts);
          });
    }

    private void FlyoutsPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.OriginalSource is DependencyObject element)
      {
        // no preview if we just clicked these elements
        if (element.TryFindParent<Flyout>() != null
            || Equals(element, overlayBox)
            || element.TryFindParent<CrystalDialogBase>() != null
            || Equals(element.TryFindParent<ContentControl>(), icon)
            || element.TryFindParent<WindowCommands>() != null
            || element.TryFindParent<WindowButtonCommands>() != null)
        {
          return;
        }
      }

      if (Flyouts!.OverrideExternalCloseButton is null)
      {
        foreach (var flyout in Flyouts.GetFlyouts().Where(x => x.IsOpen && x.ExternalCloseButton == e.ChangedButton && (!x.IsPinned || Flyouts.OverrideIsPinned)))
        {
          flyout.SetCurrentValue(Flyout.IsOpenProperty, BooleanBoxes.FalseBox);
        }
      }
      else if (Flyouts.OverrideExternalCloseButton == e.ChangedButton)
      {
        foreach (var flyout in Flyouts.GetFlyouts().Where(x => x.IsOpen && (!x.IsPinned || Flyouts.OverrideIsPinned)))
        {
          flyout.SetCurrentValue(Flyout.IsOpenProperty, BooleanBoxes.FalseBox);
        }
      }
    }

    private static void UpdateLogicalChildren(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (dependencyObject is not CrystalWindow window)
      {
        return;
      }

      if (e.OldValue is FrameworkElement oldChild)
      {
        window.RemoveLogicalChild(oldChild);
      }

      if (e.NewValue is FrameworkElement newChild)
      {
        window.AddLogicalChild(newChild);
        // Yes, that's crazy. But we must do this to enable all possible scenarios for setting DataContext
        // in a Window. Without set the DataContext at this point it can happen that e.g. a Flyout
        // doesn't get the same DataContext.
        // So now we can type
        //
        // this.InitializeComponent();
        // this.DataContext = new MainViewModel();
        //
        // or
        //
        // this.DataContext = new MainViewModel();
        // this.InitializeComponent();
        //
        newChild.DataContext = window.DataContext;
      }
    }

    /// <inheritdoc/>
    protected override IEnumerator LogicalChildren
    {
      get
      {
        // cheat, make a list with all logical content and return the enumerator
        ArrayList children = new ArrayList();
        if (Content != null)
        {
          children.Add(Content);
        }

        if (LeftWindowCommands != null)
        {
          children.Add(LeftWindowCommands);
        }

        if (RightWindowCommands != null)
        {
          children.Add(RightWindowCommands);
        }

        if (WindowButtonCommands != null)
        {
          children.Add(WindowButtonCommands);
        }

        if (Flyouts != null)
        {
          children.Add(Flyouts);
        }

        return children.GetEnumerator();
      }
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      LeftWindowCommandsPresenter = GetTemplateChild(PART_LeftWindowCommands) as ContentPresenter;
      RightWindowCommandsPresenter = GetTemplateChild(PART_RightWindowCommands) as ContentPresenter;
      WindowButtonCommandsPresenter = GetTemplateChild(PART_WindowButtonCommands) as ContentPresenter;

      LeftWindowCommands ??= new WindowCommands();
      RightWindowCommands ??= new WindowCommands();
      WindowButtonCommands ??= new WindowButtonCommands();

      LeftWindowCommands.SetValue(WindowCommands.ParentWindowPropertyKey, this);
      RightWindowCommands.SetValue(WindowCommands.ParentWindowPropertyKey, this);
      WindowButtonCommands.SetValue(WindowButtonCommands.ParentWindowPropertyKey, this);

      overlayBox = GetTemplateChild(PART_OverlayBox) as Grid;
      crystalActiveDialogContainer = GetTemplateChild(PART_CrystalActiveDialogContainer) as Grid;
      crystalInactiveDialogContainer = GetTemplateChild(PART_CrystalInactiveDialogsContainer) as Grid;
      flyoutModal = GetTemplateChild(PART_FlyoutModal) as Rectangle;

      if (flyoutModal is not null)
      {
        flyoutModal.PreviewMouseDown += FlyoutsPreviewMouseDown;
      }

      PreviewMouseDown += FlyoutsPreviewMouseDown;

      icon = GetTemplateChild(PART_Icon) as FrameworkElement;
      titleBar = GetTemplateChild(PART_TitleBar) as UIElement;
      titleBarBackground = GetTemplateChild(PART_WindowTitleBackground) as UIElement;
      windowTitleThumb = GetTemplateChild(PART_WindowTitleThumb) as Thumb;
      flyoutModalDragMoveThumb = GetTemplateChild(PART_FlyoutModalDragMoveThumb) as Thumb;

      UpdateTitleBarElementsVisibility();

      if (GetTemplateChild(PART_Content) is CrystalContentControl crystalContentControl)
      {
        crystalContentControl.TransitionCompleted += (_, _) => RaiseEvent(new RoutedEventArgs(WindowTransitionCompletedEvent));
      }
    }

    /// <summary>
    /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new CrystalWindowAutomationPeer(this);
    }

    protected internal IntPtr CriticalHandle
    {
      get
      {
        VerifyAccess();
        var value = typeof(Window).GetProperty("CriticalHandle", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(this, new object[0]) ?? IntPtr.Zero;
        return (IntPtr)value;
      }
    }

    private void ClearWindowEvents()
    {
      if (windowTitleThumb != null)
      {
        windowTitleThumb.PreviewMouseLeftButtonUp -= WindowTitleThumbOnPreviewMouseLeftButtonUp;
        windowTitleThumb.DragDelta -= WindowTitleThumbMoveOnDragDelta;
        windowTitleThumb.MouseDoubleClick -= WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
        windowTitleThumb.MouseRightButtonUp -= WindowTitleThumbSystemMenuOnMouseRightButtonUp;
      }

      if (titleBar is ICrystalThumb thumbContentControl)
      {
        thumbContentControl.PreviewMouseLeftButtonUp -= WindowTitleThumbOnPreviewMouseLeftButtonUp;
        thumbContentControl.DragDelta -= WindowTitleThumbMoveOnDragDelta;
        thumbContentControl.MouseDoubleClick -= WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
        thumbContentControl.MouseRightButtonUp -= WindowTitleThumbSystemMenuOnMouseRightButtonUp;
      }

      if (flyoutModalDragMoveThumb != null)
      {
        flyoutModalDragMoveThumb.PreviewMouseLeftButtonUp -= WindowTitleThumbOnPreviewMouseLeftButtonUp;
        flyoutModalDragMoveThumb.DragDelta -= WindowTitleThumbMoveOnDragDelta;
        flyoutModalDragMoveThumb.MouseDoubleClick -= WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
        flyoutModalDragMoveThumb.MouseRightButtonUp -= WindowTitleThumbSystemMenuOnMouseRightButtonUp;
      }

      if (icon != null)
      {
        icon.MouseDown -= IconMouseDown;
      }

      SizeChanged -= CrystalWindow_SizeChanged;
    }

    private void SetWindowEvents()
    {
      // clear all event handlers first
      ClearWindowEvents();

      // set mouse down/up for icon
      if (icon != null && icon.Visibility == Visibility.Visible)
      {
        icon.MouseDown += IconMouseDown;
      }

      if (windowTitleThumb != null)
      {
        windowTitleThumb.PreviewMouseLeftButtonUp += WindowTitleThumbOnPreviewMouseLeftButtonUp;
        windowTitleThumb.DragDelta += WindowTitleThumbMoveOnDragDelta;
        windowTitleThumb.MouseDoubleClick += WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
        windowTitleThumb.MouseRightButtonUp += WindowTitleThumbSystemMenuOnMouseRightButtonUp;
      }

      if (titleBar is ICrystalThumb thumbContentControl)
      {
        thumbContentControl.PreviewMouseLeftButtonUp += WindowTitleThumbOnPreviewMouseLeftButtonUp;
        thumbContentControl.DragDelta += WindowTitleThumbMoveOnDragDelta;
        thumbContentControl.MouseDoubleClick += WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
        thumbContentControl.MouseRightButtonUp += WindowTitleThumbSystemMenuOnMouseRightButtonUp;
      }

      if (flyoutModalDragMoveThumb != null)
      {
        flyoutModalDragMoveThumb.PreviewMouseLeftButtonUp += WindowTitleThumbOnPreviewMouseLeftButtonUp;
        flyoutModalDragMoveThumb.DragDelta += WindowTitleThumbMoveOnDragDelta;
        flyoutModalDragMoveThumb.MouseDoubleClick += WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
        flyoutModalDragMoveThumb.MouseRightButtonUp += WindowTitleThumbSystemMenuOnMouseRightButtonUp;
      }

      // handle size if we have a Grid for the title (e.g. clean window have a centered title)
      if (titleBar != null && TitleAlignment == HorizontalAlignment.Center)
      {
        SizeChanged += CrystalWindow_SizeChanged;
      }
    }

    private void IconMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
      {
        if (e.ClickCount == 2)
        {
          Close();
        }
        else if (ShowSystemMenu)
        {
#pragma warning disable 618
          Windows.Shell.SystemCommands.ShowSystemMenuPhysicalCoordinates(this, PointToScreen(new Point(BorderThickness.Left, TitleBarHeight + BorderThickness.Top)));
#pragma warning restore 618
        }
      }
    }

    private void WindowTitleThumbOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      DoWindowTitleThumbOnPreviewMouseLeftButtonUp(this, e);
    }

    private void WindowTitleThumbMoveOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
    {
      DoWindowTitleThumbMoveOnDragDelta(sender as ICrystalThumb, this, dragDeltaEventArgs);
    }

    private void WindowTitleThumbChangeWindowStateOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(this, mouseButtonEventArgs);
    }

    private void WindowTitleThumbSystemMenuOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(this, e);
    }

    internal static void DoWindowTitleThumbOnPreviewMouseLeftButtonUp(CrystalWindow window, MouseButtonEventArgs mouseButtonEventArgs)
    {
      if (mouseButtonEventArgs.Source == mouseButtonEventArgs.OriginalSource)
      {
        Mouse.Capture(null);
      }
    }

    internal static void DoWindowTitleThumbMoveOnDragDelta(ICrystalThumb? thumb, CrystalWindow? window, DragDeltaEventArgs dragDeltaEventArgs)
    {
      if (thumb is null)
      {
        throw new ArgumentNullException(nameof(thumb));
      }

      if (window is null)
      {
        throw new ArgumentNullException(nameof(window));
      }

      // drag only if IsWindowDraggable is set to true
      if (!window.IsWindowDraggable ||
          (!(Math.Abs(dragDeltaEventArgs.HorizontalChange) > 2) && !(Math.Abs(dragDeltaEventArgs.VerticalChange) > 2)))
      {
        return;
      }

      // This was taken from DragMove internal code
      window.VerifyAccess();

      // if the window is maximized dragging is only allowed on title bar (also if not visible)
      var windowIsMaximized = window.WindowState == WindowState.Maximized;
      var isMouseOnTitlebar = Mouse.GetPosition(thumb).Y <= window.TitleBarHeight && window.TitleBarHeight > 0;
      if (!isMouseOnTitlebar && windowIsMaximized)
      {
        return;
      }

#pragma warning disable 618
      // for the touch usage
      UnsafeNativeMethods.ReleaseCapture();
#pragma warning restore 618

      if (windowIsMaximized)
      {
        EventHandler? onWindowStateChanged = null;
        onWindowStateChanged = (sender, args) =>
            {
              window.StateChanged -= onWindowStateChanged;

              if (window.WindowState == WindowState.Normal)
              {
                Mouse.Capture(thumb, CaptureMode.Element);
              }
            };

        window.StateChanged -= onWindowStateChanged;
        window.StateChanged += onWindowStateChanged;
      }

#pragma warning disable 618
      // these lines are from DragMove
      // NativeMethods.SendMessage(criticalHandle, WM.SYSCOMMAND, (IntPtr)SC.MOUSEMOVE, IntPtr.Zero);
      // NativeMethods.SendMessage(criticalHandle, WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

      var wpfPoint = window.PointToScreen(Mouse.GetPosition(window));
      var x = (int)wpfPoint.X;
      var y = (int)wpfPoint.Y;
      NativeMethods.SendMessage(window.CriticalHandle, WM.NCLBUTTONDOWN, (IntPtr)HT.CAPTION, new IntPtr(x | (y << 16)));
#pragma warning restore 618
    }

    internal static void DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(CrystalWindow window, MouseButtonEventArgs mouseButtonEventArgs)
    {
      // restore/maximize only with left button
      if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
      {
        // we can maximize or restore the window if the title bar height is set (also if title bar is hidden)
        var canResize = window.ResizeMode == ResizeMode.CanResizeWithGrip || window.ResizeMode == ResizeMode.CanResize;
        var mousePos = Mouse.GetPosition(window);
        var isMouseOnTitlebar = mousePos.Y <= window.TitleBarHeight && window.TitleBarHeight > 0;
        if (canResize && isMouseOnTitlebar)
        {
#pragma warning disable 618
          if (window.WindowState == WindowState.Normal)
          {
            Windows.Shell.SystemCommands.MaximizeWindow(window);
          }
          else
          {
            Windows.Shell.SystemCommands.RestoreWindow(window);
          }
#pragma warning restore 618
          mouseButtonEventArgs.Handled = true;
        }
      }
    }

    internal static void DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(CrystalWindow window, MouseButtonEventArgs e)
    {
      if (window.ShowSystemMenuOnRightClick)
      {
        // show menu only if mouse pos is on title bar or if we have a window with none style and no title bar
        var mousePos = e.GetPosition(window);
        if ((mousePos.Y <= window.TitleBarHeight && window.TitleBarHeight > 0) || (window.UseNoneWindowStyle && window.TitleBarHeight <= 0))
        {
#pragma warning disable 618
          Windows.Shell.SystemCommands.ShowSystemMenuPhysicalCoordinates(window, window.PointToScreen(mousePos));
#pragma warning restore 618
        }
      }
    }

    /// <summary>
    /// Gets the template child with the given name.
    /// </summary>
    /// <typeparam name="T">The interface type inherited from DependencyObject.</typeparam>
    /// <param name="name">The name of the template child.</param>
    internal T? GetPart<T>(string name)
        where T : class
    {
      return GetTemplateChild(name) as T;
    }

    internal void HandleFlyoutStatusChange(Flyout flyout, IList<Flyout> visibleFlyouts)
    {
      // Checks a recently opened Flyout's position.
      var zIndex = flyout.IsOpen ? Panel.GetZIndex(flyout) + 3 : visibleFlyouts.Count() + 2;

      //if the the corresponding behavior has the right flag, set the window commands' and icon zIndex to a number that is higher than the Flyout's.
      icon?.SetValue(Panel.ZIndexProperty, flyout.IsModal && flyout.IsOpen ? 0 : (IconOverlayBehavior.HasFlag(OverlayBehavior.Flyouts) ? zIndex : 1));
      LeftWindowCommandsPresenter?.SetValue(Panel.ZIndexProperty, flyout.IsModal && flyout.IsOpen ? 0 : 1);
      RightWindowCommandsPresenter?.SetValue(Panel.ZIndexProperty, flyout.IsModal && flyout.IsOpen ? 0 : 1);
      WindowButtonCommandsPresenter?.SetValue(Panel.ZIndexProperty, flyout.IsModal && flyout.IsOpen ? 0 : (WindowButtonCommandsOverlayBehavior.HasFlag(OverlayBehavior.Flyouts) ? zIndex : 1));

      this.HandleWindowCommandsForFlyouts(visibleFlyouts);

      if (flyoutModal != null)
      {
        flyoutModal.Visibility = visibleFlyouts.Any(x => x.IsModal) ? Visibility.Visible : Visibility.Hidden;
      }

      RaiseEvent(new FlyoutStatusChangedRoutedEventArgs(FlyoutsStatusChangedEvent, this) { ChangedFlyout = flyout });
    }

    public class FlyoutStatusChangedRoutedEventArgs : RoutedEventArgs
    {
      internal FlyoutStatusChangedRoutedEventArgs(RoutedEvent rEvent, object source)
          : base(rEvent, source)
      {
      }

      public Flyout? ChangedFlyout { get; internal set; }
    }
  }
}