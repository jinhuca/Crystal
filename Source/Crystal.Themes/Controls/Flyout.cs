using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ControlzEx;
using ControlzEx.Theming;
using Crystal.Themes.Automation.Peers;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// A sliding panel control that is hosted in a <see cref="CrystalWindow"/> via a <see cref="FlyoutsControl"/>.
  /// </summary>
  [TemplatePart(Name = "PART_Root", Type = typeof(FrameworkElement))]
  [TemplatePart(Name = "PART_Header", Type = typeof(FrameworkElement))]
  [TemplatePart(Name = "PART_Content", Type = typeof(FrameworkElement))]
  public class Flyout : HeaderedContentControl
  {
    public static readonly RoutedEvent IsOpenChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(IsOpenChanged),
                                         RoutingStrategy.Bubble,
                                         typeof(RoutedEventHandler),
                                         typeof(Flyout));

    /// <summary>
    /// An event that is raised when <see cref="IsOpen"/> property changes.
    /// </summary>
    public event RoutedEventHandler IsOpenChanged
    {
      add => AddHandler(IsOpenChangedEvent, value);
      remove => RemoveHandler(IsOpenChangedEvent, value);
    }

    public static readonly RoutedEvent OpeningFinishedEvent =
        EventManager.RegisterRoutedEvent(nameof(OpeningFinished),
                                         RoutingStrategy.Bubble,
                                         typeof(RoutedEventHandler),
                                         typeof(Flyout));

    /// <summary>
    /// An event that is raised when the opening animation has finished.
    /// </summary>
    public event RoutedEventHandler OpeningFinished
    {
      add => AddHandler(OpeningFinishedEvent, value);
      remove => RemoveHandler(OpeningFinishedEvent, value);
    }

    public static readonly RoutedEvent ClosingFinishedEvent =
        EventManager.RegisterRoutedEvent(nameof(ClosingFinished),
                                         RoutingStrategy.Bubble,
                                         typeof(RoutedEventHandler),
                                         typeof(Flyout));

    /// <summary>
    /// An event that is raised when the closing animation has finished.
    /// </summary>
    public event RoutedEventHandler ClosingFinished
    {
      add => AddHandler(ClosingFinishedEvent, value);
      remove => RemoveHandler(ClosingFinishedEvent, value);
    }

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(nameof(Position),
                                    typeof(Position),
                                    typeof(Flyout),
                                    new PropertyMetadata(Position.Left, OnPositionPropertyChanged));

    private static void OnPositionPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      var flyout = (Flyout)dependencyObject;
      var wasOpen = flyout.IsOpen;
      if (wasOpen && flyout.AnimateOnPositionChange)
      {
        flyout.ApplyAnimation((Position)e.NewValue, flyout.AnimateOpacity);
        VisualStateManager.GoToState(flyout, "Hide", true);
      }
      else
      {
        flyout.ApplyAnimation((Position)e.NewValue, flyout.AnimateOpacity, false);
      }

      if (wasOpen && flyout.AnimateOnPositionChange)
      {
        flyout.ApplyAnimation((Position)e.NewValue, flyout.AnimateOpacity);
        VisualStateManager.GoToState(flyout, "Show", true);
      }
    }

    /// <summary>
    /// Gets or sets the position of this <see cref="Flyout"/> inside the <see cref="FlyoutsControl"/>.
    /// </summary>
    public Position Position
    {
      get => (Position)GetValue(PositionProperty);
      set => SetValue(PositionProperty, value);
    }

    public static readonly DependencyProperty IsPinnedProperty
        = DependencyProperty.Register(nameof(IsPinned),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether this <see cref="Flyout"/> stays open when the user clicks somewhere outside of it.
    /// </summary>
    public bool IsPinned
    {
      get => (bool)GetValue(IsPinnedProperty);
      set => SetValue(IsPinnedProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty IsOpenProperty
        = DependencyProperty.Register(nameof(IsOpen),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenPropertyChanged));

    private static void OnIsOpenPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      var flyout = (Flyout)dependencyObject;

      Action openedChangedAction = () =>
          {
            if (e.NewValue != e.OldValue)
            {
              if (flyout.AreAnimationsEnabled)
              {
                if ((bool)e.NewValue)
                {
                  if (flyout.hideStoryboard != null)
                  {
                          // don't let the storyboard end it's completed event
                          // otherwise it could be hidden on start
                          flyout.hideStoryboard.Completed -= flyout.HideStoryboardCompleted;
                  }

                  flyout.Visibility = Visibility.Visible;
                  flyout.ApplyAnimation(flyout.Position, flyout.AnimateOpacity);
                  flyout.TryFocusElement();
                  if (flyout.showStoryboard != null)
                  {
                    flyout.showStoryboard.Completed += flyout.ShowStoryboardCompleted;
                  }
                  else
                  {
                    flyout.Shown();
                  }

                  if (flyout.IsAutoCloseEnabled)
                  {
                    flyout.StartAutoCloseTimer();
                  }
                }
                else
                {
                  if (flyout.showStoryboard != null)
                  {
                    flyout.showStoryboard.Completed -= flyout.ShowStoryboardCompleted;
                  }

                  flyout.StopAutoCloseTimer();
                  flyout.SetValue(IsShownPropertyKey, BooleanBoxes.FalseBox);
                  if (flyout.hideStoryboard != null)
                  {
                    flyout.hideStoryboard.Completed += flyout.HideStoryboardCompleted;
                  }
                  else
                  {
                    flyout.Hide();
                  }
                }

                VisualStateManager.GoToState(flyout, (bool)e.NewValue == false ? "Hide" : "Show", true);
              }
              else
              {
                if ((bool)e.NewValue)
                {
                  flyout.Visibility = Visibility.Visible;
                  flyout.TryFocusElement();
                  flyout.Shown();
                  if (flyout.IsAutoCloseEnabled)
                  {
                    flyout.StartAutoCloseTimer();
                  }
                }
                else
                {
                  flyout.StopAutoCloseTimer();
                  flyout.SetValue(IsShownPropertyKey, BooleanBoxes.FalseBox);
                  flyout.Hide();
                }

                VisualStateManager.GoToState(flyout, (bool)e.NewValue == false ? "HideDirect" : "ShowDirect", true);
              }
            }

            flyout.RaiseEvent(new RoutedEventArgs(IsOpenChangedEvent));
          };

      flyout.Dispatcher.BeginInvoke(DispatcherPriority.Background, openedChangedAction);
    }

    /// <summary>
    /// Gets or sets whether this <see cref="Flyout"/> should be visible or not.
    /// </summary>
    public bool IsOpen
    {
      get => (bool)GetValue(IsOpenProperty);
      set => SetValue(IsOpenProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="IsShown"/> dependency property.</summary>
    private static readonly DependencyPropertyKey IsShownPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(IsShown),
                                              typeof(bool),
                                              typeof(Flyout),
                                              new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>Identifies the <see cref="IsShown"/> dependency property.</summary>
    public static readonly DependencyProperty IsShownProperty = IsShownPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets whether the <see cref="Flyout"/> is completely shown (after <see cref="IsOpen"/> was set to true).
    /// </summary>
    public bool IsShown
    {
      get => (bool)GetValue(IsShownProperty);
      protected set => SetValue(IsShownPropertyKey, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty AnimateOnPositionChangeProperty
        = DependencyProperty.Register(nameof(AnimateOnPositionChange),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets whether this <see cref="Flyout"/> uses the open/close animation when changing the <see cref="Position"/> property (default is true).
    /// </summary>
    public bool AnimateOnPositionChange
    {
      get => (bool)GetValue(AnimateOnPositionChangeProperty);
      set => SetValue(AnimateOnPositionChangeProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty AnimateOpacityProperty
        = DependencyProperty.Register(nameof(AnimateOpacity),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, (d, args) => (d as Flyout)?.UpdateOpacityChange()));

    /// <summary>
    /// Gets or sets whether this <see cref="Flyout"/> animates the opacity when opening/closing the <see cref="Flyout"/>.
    /// </summary>
    public bool AnimateOpacity
    {
      get => (bool)GetValue(AnimateOpacityProperty);
      set => SetValue(AnimateOpacityProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty IsModalProperty
        = DependencyProperty.Register(nameof(IsModal),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets whether this <see cref="Flyout"/> is modal.
    /// </summary>
    public bool IsModal
    {
      get => (bool)GetValue(IsModalProperty);
      set => SetValue(IsModalProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty CloseCommandProperty
        = DependencyProperty.RegisterAttached(nameof(CloseCommand),
                                              typeof(ICommand),
                                              typeof(Flyout),
                                              new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets a <see cref="ICommand"/> which will be executed if the close button was clicked.
    /// </summary>
    /// <remarks>
    /// The <see cref="ICommand"/> won't be executed when <see cref="IsOpen"/> property will be set to false/true.
    /// </remarks>
    public ICommand? CloseCommand
    {
      get => (ICommand?)GetValue(CloseCommandProperty);
      set => SetValue(CloseCommandProperty, value);
    }

    public static readonly DependencyProperty CloseCommandParameterProperty
        = DependencyProperty.Register(nameof(CloseCommandParameter),
                                      typeof(object),
                                      typeof(Flyout),
                                      new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the parameter for the <see cref="CloseCommand"/>.
    /// </summary>
    public object? CloseCommandParameter
    {
      get => GetValue(CloseCommandParameterProperty);
      set => SetValue(CloseCommandParameterProperty, value);
    }

    public static readonly DependencyProperty ThemeProperty
        = DependencyProperty.Register(nameof(Theme),
                                      typeof(FlyoutTheme),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(FlyoutTheme.Dark, (d, args) => (d as Flyout)?.UpdateFlyoutTheme()));

    /// <summary>
    /// Gets or sets the theme for the <see cref="Flyout"/>.
    /// </summary>
    public FlyoutTheme Theme
    {
      get => (FlyoutTheme)GetValue(ThemeProperty);
      set => SetValue(ThemeProperty, value);
    }

    public static readonly DependencyProperty ExternalCloseButtonProperty
        = DependencyProperty.Register(nameof(ExternalCloseButton),
                                      typeof(MouseButton),
                                      typeof(Flyout),
                                      new PropertyMetadata(MouseButton.Left));

    /// <summary>
    /// Gets or sets the mouse button that closes the <see cref="Flyout"/> when the user clicks somewhere outside of it.
    /// </summary>
    public MouseButton ExternalCloseButton
    {
      get => (MouseButton)GetValue(ExternalCloseButtonProperty);
      set => SetValue(ExternalCloseButtonProperty, value);
    }

    public static readonly DependencyProperty CloseButtonVisibilityProperty
        = DependencyProperty.Register(nameof(CloseButtonVisibility),
                                      typeof(Visibility),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the close button for this <see cref="Flyout"/>.
    /// </summary>
    public Visibility CloseButtonVisibility
    {
      get => (Visibility)GetValue(CloseButtonVisibilityProperty);
      set => SetValue(CloseButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty CloseButtonIsCancelProperty
        = DependencyProperty.Register(nameof(CloseButtonIsCancel),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets a value that indicates whether the close button is a Cancel button. A user can activate the Cancel button by pressing the ESC key.
    /// </summary>
    public bool CloseButtonIsCancel
    {
      get => (bool)GetValue(CloseButtonIsCancelProperty);
      set => SetValue(CloseButtonIsCancelProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty TitleVisibilityProperty
        = DependencyProperty.Register(nameof(TitleVisibility),
                                      typeof(Visibility),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the title.
    /// </summary>
    public Visibility TitleVisibility
    {
      get => (Visibility)GetValue(TitleVisibilityProperty);
      set => SetValue(TitleVisibilityProperty, value);
    }

    public static readonly DependencyProperty AreAnimationsEnabledProperty
        = DependencyProperty.Register(nameof(AreAnimationsEnabled),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets a value that indicates whether the <see cref="Flyout"/> uses animations for open/close.
    /// </summary>
    public bool AreAnimationsEnabled
    {
      get => (bool)GetValue(AreAnimationsEnabledProperty);
      set => SetValue(AreAnimationsEnabledProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty FocusedElementProperty
        = DependencyProperty.Register(nameof(FocusedElement),
                                      typeof(FrameworkElement),
                                      typeof(Flyout),
                                      new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the focused element.
    /// </summary>
    public FrameworkElement? FocusedElement
    {
      get => (FrameworkElement?)GetValue(FocusedElementProperty);
      set => SetValue(FocusedElementProperty, value);
    }

    public static readonly DependencyProperty AllowFocusElementProperty
        = DependencyProperty.Register(nameof(AllowFocusElement),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="Flyout"/> should try focus an element.
    /// </summary>
    public bool AllowFocusElement
    {
      get => (bool)GetValue(AllowFocusElementProperty);
      set => SetValue(AllowFocusElementProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty IsAutoCloseEnabledProperty
        = DependencyProperty.Register(nameof(IsAutoCloseEnabled),
                                      typeof(bool),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, OnIsAutoCloseEnabledPropertyChanged));

    private static void OnIsAutoCloseEnabledPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      var flyout = (Flyout)dependencyObject;

      Action autoCloseEnabledChangedAction = () =>
          {
            if (e.NewValue != e.OldValue)
            {
              if ((bool)e.NewValue)
              {
                if (flyout.IsOpen)
                {
                  flyout.StartAutoCloseTimer();
                }
              }
              else
              {
                flyout.StopAutoCloseTimer();
              }
            }
          };

      flyout.Dispatcher.BeginInvoke(DispatcherPriority.Background, autoCloseEnabledChangedAction);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="Flyout"/> should auto close after the <see cref="AutoCloseInterval"/> has passed.
    /// </summary>
    public bool IsAutoCloseEnabled
    {
      get => (bool)GetValue(IsAutoCloseEnabledProperty);
      set => SetValue(IsAutoCloseEnabledProperty, BooleanBoxes.Box(value));
    }

    public static readonly DependencyProperty AutoCloseIntervalProperty
        = DependencyProperty.Register(nameof(AutoCloseInterval),
                                      typeof(long),
                                      typeof(Flyout),
                                      new FrameworkPropertyMetadata(5000L, AutoCloseIntervalChanged));

    private static void AutoCloseIntervalChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      var flyout = (Flyout)dependencyObject;

      Action autoCloseIntervalChangedAction = () =>
          {
            if (e.NewValue != e.OldValue)
            {
              flyout.InitializeAutoCloseTimer();
              if (flyout.IsAutoCloseEnabled && flyout.IsOpen)
              {
                flyout.StartAutoCloseTimer();
              }
            }
          };

      flyout.Dispatcher.BeginInvoke(DispatcherPriority.Background, autoCloseIntervalChangedAction);
    }

    /// <summary>
    /// Gets or sets the time in milliseconds when the <see cref="Flyout"/> should auto close.
    /// </summary>
    public long AutoCloseInterval
    {
      get => (long)GetValue(AutoCloseIntervalProperty);
      set => SetValue(AutoCloseIntervalProperty, value);
    }

    /// <summary>Identifies the <see cref="Owner"/> dependency property.</summary>
    private static readonly DependencyPropertyKey OwnerPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Owner),
                                            typeof(FlyoutsControl),
                                            typeof(Flyout),
                                            new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Owner"/> dependency property.</summary>
    public static readonly DependencyProperty OwnerProperty = OwnerPropertyKey.DependencyProperty;

    public FlyoutsControl? Owner
    {
      get => (FlyoutsControl?)GetValue(OwnerProperty);
      protected set => SetValue(OwnerPropertyKey, value);
    }

    private DispatcherTimer? autoCloseTimer;
    private FrameworkElement? flyoutRoot;
    private Storyboard? showStoryboard;
    private Storyboard? hideStoryboard;
    private SplineDoubleKeyFrame? hideFrame;
    private SplineDoubleKeyFrame? hideFrameY;
    private SplineDoubleKeyFrame? showFrame;
    private SplineDoubleKeyFrame? showFrameY;
    private SplineDoubleKeyFrame? fadeOutFrame;
    private FrameworkElement? flyoutHeader;
    private FrameworkElement? flyoutContent;
    private CrystalWindow? parentWindow;

    private CrystalWindow? ParentWindow => parentWindow ??= this.TryFindParent<CrystalWindow>();

    /// <summary>
    /// <see cref="IsOpen"/> property changed notifier used in <see cref="FlyoutsControl"/>.
    /// </summary>
    internal PropertyChangeNotifier? IsOpenPropertyChangeNotifier { get; set; }

    /// <summary>
    /// <see cref="Theme"/> property changed notifier used in <see cref="FlyoutsControl"/>.
    /// </summary>
    internal PropertyChangeNotifier? ThemePropertyChangeNotifier { get; set; }

    static Flyout()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Flyout), new FrameworkPropertyMetadata(typeof(Flyout)));
    }

    public Flyout()
    {
      Loaded += (sender, args) => UpdateFlyoutTheme();
      InitializeAutoCloseTimer();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new FlyoutAutomationPeer(this);
    }

    private void InitializeAutoCloseTimer()
    {
      StopAutoCloseTimer();

      autoCloseTimer = new DispatcherTimer();
      autoCloseTimer.Tick += AutoCloseTimerCallback;
      autoCloseTimer.Interval = TimeSpan.FromMilliseconds(AutoCloseInterval);
    }

    private void StartAutoCloseTimer()
    {
      // in case it is already running
      StopAutoCloseTimer();

      if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
      {
        autoCloseTimer?.Start();
      }
    }

    private void StopAutoCloseTimer()
    {
      if (autoCloseTimer != null && autoCloseTimer.IsEnabled)
      {
        autoCloseTimer.Stop();
      }
    }

    private void AutoCloseTimerCallback(object? sender, EventArgs e)
    {
      StopAutoCloseTimer();

      // if the flyout is open and auto close is still enabled then close the flyout
      if (IsOpen && IsAutoCloseEnabled)
      {
        SetCurrentValue(IsOpenProperty, BooleanBoxes.FalseBox);
      }
    }

    private void UpdateFlyoutTheme()
    {
      if (IsLoaded == false)
      {
        return;
      }

      var flyoutsControl = Owner ?? this.TryFindParent<FlyoutsControl>();

      if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
      {
        SetCurrentValue(VisibilityProperty, flyoutsControl != null ? Visibility.Collapsed : Visibility.Visible);
      }

      var window = ParentWindow;
      if (window != null)
      {
        var windowTheme = DetectTheme(this);

        if (windowTheme != null)
        {
          ChangeFlyoutTheme(windowTheme);
        }

        // we must certain to get the right foreground for window commands and buttons
        if (flyoutsControl != null && IsOpen)
        {
          flyoutsControl.HandleFlyoutStatusChange(this, window);
        }
      }
    }

    private static ControlzEx.Theming.Theme? DetectTheme(Flyout? flyout)
    {
      if (flyout is null)
      {
        return null;
      }

      // first look for owner
      var window = flyout.ParentWindow;
      var theme = window != null ? ThemeManager.Current.DetectTheme(window) : null;
      if (theme != null)
      {
        return theme;
      }

      // second try, look for main window and then for current application
      if (Application.Current != null)
      {
        theme = Application.Current.MainWindow is null
            ? ThemeManager.Current.DetectTheme(Application.Current)
            : ThemeManager.Current.DetectTheme(Application.Current.MainWindow);
        if (theme != null)
        {
          return theme;
        }
      }

      return null;
    }

    internal void ChangeFlyoutTheme(ControlzEx.Theming.Theme windowTheme)
    {
      if (windowTheme == ThemeManager.Current.DetectTheme(Resources))
      {
        return;
      }

      switch (Theme)
      {
        case FlyoutTheme.Accent:
          ThemeManager.Current.ApplyThemeResourcesFromTheme(Resources, windowTheme);
          OverrideFlyoutResources(Resources, true);
          break;

        case FlyoutTheme.Adapt:
          ThemeManager.Current.ApplyThemeResourcesFromTheme(Resources, windowTheme);
          OverrideFlyoutResources(Resources);
          break;

        case FlyoutTheme.Inverse:
          var inverseTheme = ThemeManager.Current.GetInverseTheme(windowTheme);
          if (inverseTheme is null)
          {
            throw new InvalidOperationException("The inverse Flyout theme only works if the window theme abides the naming convention. " +
                                                "See ThemeManager.GetInverseAppTheme for more infos");
          }

          ThemeManager.Current.ApplyThemeResourcesFromTheme(Resources, inverseTheme);
          OverrideFlyoutResources(Resources);
          break;

        case FlyoutTheme.Dark:
          var darkTheme = windowTheme.BaseColorScheme == ThemeManager.BaseColorDark ? windowTheme : ThemeManager.Current.GetInverseTheme(windowTheme);
          if (darkTheme is null)
          {
            throw new InvalidOperationException("The Dark Flyout theme only works if the window theme abides the naming convention. " +
                                                "See ThemeManager.GetInverseAppTheme for more infos");
          }

          ThemeManager.Current.ApplyThemeResourcesFromTheme(Resources, darkTheme);
          OverrideFlyoutResources(Resources);
          break;

        case FlyoutTheme.Light:
          var lightTheme = windowTheme.BaseColorScheme == ThemeManager.BaseColorLight ? windowTheme : ThemeManager.Current.GetInverseTheme(windowTheme);
          if (lightTheme is null)
          {
            throw new InvalidOperationException("The Light Flyout theme only works if the window theme abides the naming convention. " +
                                                "See ThemeManager.GetInverseAppTheme for more infos");
          }

          ThemeManager.Current.ApplyThemeResourcesFromTheme(Resources, lightTheme);
          OverrideFlyoutResources(Resources);
          break;
      }
    }

    protected virtual void OverrideFlyoutResources(ResourceDictionary resources, bool accent = false)
    {
      var fromColorKey = accent ? "Crystal.Colors.Highlight" : "Crystal.Colors.Flyout";

      resources.BeginInit();

      var fromColor = (Color)resources[fromColorKey];
      resources["Crystal.Colors.ThemeBackground"] = fromColor;
      resources["Crystal.Colors.Flyout"] = fromColor;

      var newBrush = new SolidColorBrush(fromColor);
      newBrush.Freeze();
      resources["Crystal.Brushes.Flyout.Background"] = newBrush;
      resources["Crystal.Brushes.Control.Background"] = newBrush;
      resources["Crystal.Brushes.ThemeBackground"] = newBrush;
      resources["Crystal.Brushes.Window.Background"] = newBrush;
      resources[SystemColors.WindowBrushKey] = newBrush;

      if (accent)
      {
        fromColor = (Color)resources["Crystal.Colors.IdealForeground"];
        newBrush = new SolidColorBrush(fromColor);
        newBrush.Freeze();
        resources["Crystal.Brushes.Flyout.Foreground"] = newBrush;
        resources["Crystal.Brushes.Text"] = newBrush;

        if (resources.Contains("Crystal.Colors.AccentBase"))
        {
          fromColor = (Color)resources["Crystal.Colors.AccentBase"];
        }
        else
        {
          var accentColor = (Color)resources["Crystal.Colors.Accent"];
          fromColor = Color.FromArgb(255, accentColor.R, accentColor.G, accentColor.B);
        }

        newBrush = new SolidColorBrush(fromColor);
        newBrush.Freeze();
        resources["Crystal.Colors.Highlight"] = fromColor;
        resources["Crystal.Brushes.Highlight"] = newBrush;
      }

      resources.EndInit();
    }

    private void UpdateOpacityChange()
    {
      if (flyoutRoot is null || fadeOutFrame is null || System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
      {
        return;
      }

      if (!AnimateOpacity)
      {
        fadeOutFrame.Value = 1;
        flyoutRoot.Opacity = 1;
      }
      else
      {
        fadeOutFrame.Value = 0;
        if (!IsOpen)
        {
          flyoutRoot.Opacity = 0;
        }
      }
    }

    private void HideStoryboardCompleted(object? sender, EventArgs e)
    {
      if (hideStoryboard is not null)
      {
        hideStoryboard.Completed -= HideStoryboardCompleted;
      }

      Hide();
    }

    private void Hide()
    {
      // hide the flyout, we should get better performance and prevent showing the flyout on any resizing events
      Visibility = Visibility.Hidden;
      RaiseEvent(new RoutedEventArgs(ClosingFinishedEvent));
    }

    private void ShowStoryboardCompleted(object? sender, EventArgs e)
    {
      if (showStoryboard is not null)
      {
        showStoryboard.Completed -= ShowStoryboardCompleted;
      }

      Shown();
    }

    private void Shown()
    {
      SetValue(IsShownPropertyKey, BooleanBoxes.TrueBox);
      RaiseEvent(new RoutedEventArgs(OpeningFinishedEvent));
    }

    private void TryFocusElement()
    {
      if (AllowFocusElement)
      {
        // first focus itself
        Focus();

        if (FocusedElement != null)
        {
          FocusedElement.Focus();
        }
        else if (flyoutContent is null || !flyoutContent.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)))
        {
          flyoutHeader?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }
      }
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      var flyoutsControl = ItemsControl.ItemsControlFromItemContainer(this) as FlyoutsControl ?? this.TryFindParent<FlyoutsControl>();
      SetValue(OwnerPropertyKey, flyoutsControl);

      flyoutRoot = GetTemplateChild("PART_Root") as FrameworkElement;
      if (flyoutRoot is null)
      {
        return;
      }

      flyoutHeader = GetTemplateChild("PART_Header") as FrameworkElement;
      flyoutHeader?.ApplyTemplate();

      flyoutContent = GetTemplateChild("PART_Content") as FrameworkElement;

      if (flyoutHeader is IMetroThumb thumb)
      {
        thumb.PreviewMouseLeftButtonUp -= HeaderThumbOnPreviewMouseLeftButtonUp;
        thumb.DragDelta -= HeaderThumbMoveOnDragDelta;
        thumb.MouseDoubleClick -= HeaderThumbChangeWindowStateOnMouseDoubleClick;
        thumb.MouseRightButtonUp -= HeaderThumbSystemMenuOnMouseRightButtonUp;

        thumb.PreviewMouseLeftButtonUp += HeaderThumbOnPreviewMouseLeftButtonUp;
        thumb.DragDelta += HeaderThumbMoveOnDragDelta;
        thumb.MouseDoubleClick += HeaderThumbChangeWindowStateOnMouseDoubleClick;
        thumb.MouseRightButtonUp += HeaderThumbSystemMenuOnMouseRightButtonUp;
      }

#pragma warning disable WPF0130 // Add [TemplatePart] to the type.
      showStoryboard = GetTemplateChild("ShowStoryboard") as Storyboard;
      hideStoryboard = GetTemplateChild("HideStoryboard") as Storyboard;
      hideFrame = GetTemplateChild("hideFrame") as SplineDoubleKeyFrame;
      hideFrameY = GetTemplateChild("hideFrameY") as SplineDoubleKeyFrame;
      showFrame = GetTemplateChild("showFrame") as SplineDoubleKeyFrame;
      showFrameY = GetTemplateChild("showFrameY") as SplineDoubleKeyFrame;
      fadeOutFrame = GetTemplateChild("fadeOutFrame") as SplineDoubleKeyFrame;
#pragma warning restore WPF0130 // Add [TemplatePart] to the type.

      if (hideFrame is null || showFrame is null || hideFrameY is null || showFrameY is null || fadeOutFrame is null)
      {
        return;
      }

      ApplyAnimation(Position, AnimateOpacity);
    }

    internal void CleanUp()
    {
      if (flyoutHeader is IMetroThumb thumb)
      {
        thumb.PreviewMouseLeftButtonUp -= HeaderThumbOnPreviewMouseLeftButtonUp;
        thumb.DragDelta -= HeaderThumbMoveOnDragDelta;
        thumb.MouseDoubleClick -= HeaderThumbChangeWindowStateOnMouseDoubleClick;
        thumb.MouseRightButtonUp -= HeaderThumbSystemMenuOnMouseRightButtonUp;
      }

      parentWindow = null;
    }

    private void HeaderThumbOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      var window = ParentWindow;
      if (window != null && Position != Position.Bottom)
      {
        CrystalWindow.DoWindowTitleThumbOnPreviewMouseLeftButtonUp(window, e);
      }
    }

    private void HeaderThumbMoveOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
    {
      var window = ParentWindow;
      if (window != null && Position != Position.Bottom)
      {
        CrystalWindow.DoWindowTitleThumbMoveOnDragDelta(sender as IMetroThumb, window, dragDeltaEventArgs);
      }
    }

    private void HeaderThumbChangeWindowStateOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
    {
      var window = ParentWindow;
      if (window != null && Position != Position.Bottom && Mouse.GetPosition((IInputElement)sender).Y <= window.TitleBarHeight && window.TitleBarHeight > 0)
      {
        CrystalWindow.DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(window, mouseButtonEventArgs);
      }
    }

    private void HeaderThumbSystemMenuOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      var window = ParentWindow;
      if (window != null && Position != Position.Bottom && Mouse.GetPosition((IInputElement)sender).Y <= window.TitleBarHeight && window.TitleBarHeight > 0)
      {
        CrystalWindow.DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(window, e);
      }
    }

    internal void ApplyAnimation(Position position, bool animateOpacity, bool resetShowFrame = true)
    {
      if (flyoutRoot is null || hideFrame is null || showFrame is null || hideFrameY is null || showFrameY is null || fadeOutFrame is null)
      {
        return;
      }

      if (Position == Position.Left || Position == Position.Right)
      {
        showFrame.Value = 0;
      }

      if (Position == Position.Top || Position == Position.Bottom)
      {
        showFrameY.Value = 0;
      }

      if (!animateOpacity)
      {
        fadeOutFrame.Value = 1;
        flyoutRoot.Opacity = 1;
      }
      else
      {
        fadeOutFrame.Value = 0;
        if (!IsOpen)
        {
          flyoutRoot.Opacity = 0;
        }
      }

      switch (position)
      {
        default:
          HorizontalAlignment = Margin.Right <= 0 ? HorizontalContentAlignment != HorizontalAlignment.Stretch ? HorizontalAlignment.Left : HorizontalContentAlignment : HorizontalAlignment.Stretch;
          VerticalAlignment = VerticalAlignment.Stretch;
          hideFrame.Value = -flyoutRoot.ActualWidth - Margin.Left;
          if (resetShowFrame)
          {
            flyoutRoot.RenderTransform = new TranslateTransform(-flyoutRoot.ActualWidth, 0);
          }

          break;
        case Position.Right:
          HorizontalAlignment = Margin.Left <= 0 ? HorizontalContentAlignment != HorizontalAlignment.Stretch ? HorizontalAlignment.Right : HorizontalContentAlignment : HorizontalAlignment.Stretch;
          VerticalAlignment = VerticalAlignment.Stretch;
          hideFrame.Value = flyoutRoot.ActualWidth + Margin.Right;
          if (resetShowFrame)
          {
            flyoutRoot.RenderTransform = new TranslateTransform(flyoutRoot.ActualWidth, 0);
          }

          break;
        case Position.Top:
          HorizontalAlignment = HorizontalAlignment.Stretch;
          VerticalAlignment = Margin.Bottom <= 0 ? VerticalContentAlignment != VerticalAlignment.Stretch ? VerticalAlignment.Top : VerticalContentAlignment : VerticalAlignment.Stretch;
          hideFrameY.Value = -flyoutRoot.ActualHeight - 1 - Margin.Top;
          if (resetShowFrame)
          {
            flyoutRoot.RenderTransform = new TranslateTransform(0, -flyoutRoot.ActualHeight - 1);
          }

          break;
        case Position.Bottom:
          HorizontalAlignment = HorizontalAlignment.Stretch;
          VerticalAlignment = Margin.Top <= 0 ? VerticalContentAlignment != VerticalAlignment.Stretch ? VerticalAlignment.Bottom : VerticalContentAlignment : VerticalAlignment.Stretch;
          hideFrameY.Value = flyoutRoot.ActualHeight + Margin.Bottom;
          if (resetShowFrame)
          {
            flyoutRoot.RenderTransform = new TranslateTransform(0, flyoutRoot.ActualHeight);
          }

          break;
      }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);

      if (!IsOpen)
      {
        return; // no changes for invisible flyouts, ApplyAnimation is called now in visible changed event
      }

      if (!sizeInfo.WidthChanged && !sizeInfo.HeightChanged)
      {
        return;
      }

      if (flyoutRoot is null || hideFrame is null || showFrame is null || hideFrameY is null || showFrameY is null)
      {
        return; // don't bother checking IsOpen and calling ApplyAnimation
      }

      if (Position == Position.Left || Position == Position.Right)
      {
        showFrame.Value = 0;
      }

      if (Position == Position.Top || Position == Position.Bottom)
      {
        showFrameY.Value = 0;
      }

      switch (Position)
      {
        default:
          hideFrame.Value = -flyoutRoot.ActualWidth - Margin.Left;
          break;
        case Position.Right:
          hideFrame.Value = flyoutRoot.ActualWidth + Margin.Right;
          break;
        case Position.Top:
          hideFrameY.Value = -flyoutRoot.ActualHeight - 1 - Margin.Top;
          break;
        case Position.Bottom:
          hideFrameY.Value = flyoutRoot.ActualHeight + Margin.Bottom;
          break;
      }
    }
  }
}