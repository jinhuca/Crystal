#pragma warning disable 618, CA1001
using System.Security;
using System.Windows.Interop;
using Crystal.Behaviors;
using Crystal.Themes.Native;
using Crystal.Themes.Standard;

namespace Crystal.Themes.Behaviors
{
  /// <summary>
  /// With this class we can make custom window styles.
  /// </summary>
  public partial class WindowChromeBehavior : Behavior<Window>
  {
    /// <summary>Underlying HWND for the _window.</summary>
    /// <SecurityNote>
    ///   Critical : Critical member
    /// </SecurityNote>
    [SecurityCritical]
    private IntPtr windowHandle;

    /// <summary>Underlying HWND for the _window.</summary>
    /// <SecurityNote>
    ///   Critical : Critical member provides access to HWND's window messages which are critical
    /// </SecurityNote>
    [SecurityCritical]
    private HwndSource? hwndSource;

    private PropertyChangeNotifier? topMostChangeNotifier;
    private PropertyChangeNotifier? borderThicknessChangeNotifier;
    private PropertyChangeNotifier? resizeBorderThicknessChangeNotifier;
    private Thickness? savedBorderThickness;
    private Thickness? savedResizeBorderThickness;
    private bool savedTopMost;

    private bool isCleanedUp;

    private bool dpiChanged;

    private struct SystemParameterBoundProperty
    {
      public string SystemParameterPropertyName { get; set; }

      public DependencyProperty DependencyProperty { get; set; }
    }

    /// <summary>
    /// Mirror property for <see cref="ResizeBorderThickness"/>.
    /// </summary>
    public Thickness ResizeBorderThickness
    {
      get { return (Thickness)GetValue(ResizeBorderThicknessProperty); }
      set { SetValue(ResizeBorderThicknessProperty, value); }
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="ResizeBorderThickness"/>.
    /// </summary>
    public static readonly DependencyProperty ResizeBorderThicknessProperty =
        DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(WindowChromeBehavior), new PropertyMetadata(GetDefaultResizeBorderThickness()), (value) => ((Thickness)value).IsNonNegative());

    /// <summary>
    /// Defines if the Taskbar should be ignored when maximizing a Window.
    /// This only works with WindowStyle = None.
    /// </summary>
    public bool IgnoreTaskbarOnMaximize
    {
      get { return (bool)GetValue(IgnoreTaskbarOnMaximizeProperty); }
      set { SetValue(IgnoreTaskbarOnMaximizeProperty, value); }
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="IgnoreTaskbarOnMaximize"/>.
    /// </summary>
    public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty =
        DependencyProperty.Register(nameof(IgnoreTaskbarOnMaximize), typeof(bool), typeof(WindowChromeBehavior), new PropertyMetadata(false, OnIgnoreTaskbarOnMaximizeChanged));

    /// <summary>
    /// Gets/sets if the border thickness value should be kept on maximize
    /// if the MaxHeight/MaxWidth of the window is less than the monitor resolution.
    /// </summary>
    public bool KeepBorderOnMaximize
    {
      get { return (bool)GetValue(KeepBorderOnMaximizeProperty); }
      set { SetValue(KeepBorderOnMaximizeProperty, value); }
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="KeepBorderOnMaximize"/>.
    /// </summary>
    public static readonly DependencyProperty KeepBorderOnMaximizeProperty = DependencyProperty.Register(nameof(KeepBorderOnMaximize), typeof(bool), typeof(WindowChromeBehavior), new PropertyMetadata(true, OnKeepBorderOnMaximizeChanged));

    /// <summary>
    /// Gets or sets whether the resizing of the window should be tried in a way that does not cause flicker/jitter, especially when resizing from the left side.
    /// </summary>
    /// <remarks>
    /// Please note that setting this to <c>true</c> may cause resize lag and black areas appearing on some systems.
    /// </remarks>
    public bool TryToBeFlickerFree
    {
      get { return (bool)GetValue(TryToBeFlickerFreeProperty); }
      set { SetValue(TryToBeFlickerFreeProperty, value); }
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="TryToBeFlickerFree"/>.
    /// </summary>
    public static readonly DependencyProperty TryToBeFlickerFreeProperty = DependencyProperty.Register(nameof(TryToBeFlickerFree), typeof(bool), typeof(WindowChromeBehavior), new PropertyMetadata(default(bool), OnTryToBeFlickerFreeChanged));

    private static readonly DependencyPropertyKey IsNCActivePropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsNCActive), typeof(bool), typeof(WindowChromeBehavior), new PropertyMetadata(default(bool)));

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="IsNCActive"/>.
    /// </summary>
    public static readonly DependencyProperty IsNCActiveProperty = IsNCActivePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets whether the non-client area is active or not.
    /// </summary>
    public bool IsNCActive
    {
      get { return (bool)GetValue(IsNCActiveProperty); }
      private set { SetValue(IsNCActivePropertyKey, value); }
    }

    public static readonly DependencyProperty EnableMinimizeProperty = DependencyProperty.Register(nameof(EnableMinimize), typeof(bool), typeof(WindowChromeBehavior), new PropertyMetadata(true, OnEnableMinimizeChanged));

    private static void OnEnableMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue is bool showMinButton)
      {
        var behavior = (WindowChromeBehavior)d;

        behavior.UpdateMinimizeSystemMenu(showMinButton);
      }
    }

    private void UpdateMinimizeSystemMenu(bool isVisible)
    {
      if (windowHandle != IntPtr.Zero)
      {
        if (hwndSource?.IsDisposed == true || hwndSource?.RootVisual is null)
        {
          return;
        }

        if (isVisible)
        {
          _ModifyStyle(0, WS.MINIMIZEBOX);
        }
        else
        {
          _ModifyStyle(WS.MINIMIZEBOX, 0);
        }

        _UpdateSystemMenu(AssociatedObject?.WindowState);
      }
    }

    /// <summary>
    /// Gets or sets whether if the minimize button is visible and the minimize system menu is enabled.
    /// </summary>
    public bool EnableMinimize
    {
      get { return (bool)GetValue(EnableMinimizeProperty); }
      set { SetValue(EnableMinimizeProperty, value); }
    }

    public static readonly DependencyProperty EnableMaxRestoreProperty = DependencyProperty.Register(nameof(EnableMaxRestore), typeof(bool), typeof(WindowChromeBehavior), new PropertyMetadata(true, OnEnableMaxRestoreChanged));

    private static void OnEnableMaxRestoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue is bool showMaxRestoreButton)
      {
        var behavior = (WindowChromeBehavior)d;

        behavior.UpdateMaxRestoreSystemMenu(showMaxRestoreButton);
      }
    }

    private void UpdateMaxRestoreSystemMenu(bool isVisible)
    {
      if (windowHandle != IntPtr.Zero)
      {
        if (hwndSource?.IsDisposed == true || hwndSource?.RootVisual is null)
        {
          return;
        }

        if (isVisible)
        {
          _ModifyStyle(0, WS.MAXIMIZEBOX);
        }
        else
        {
          _ModifyStyle(WS.MAXIMIZEBOX, 0);
        }

        _UpdateSystemMenu(AssociatedObject?.WindowState);
      }
    }

    /// <summary>
    /// Gets or sets whether if the maximize/restore button is visible and the maximize/restore system menu is enabled.
    /// </summary>
    public bool EnableMaxRestore
    {
      get { return (bool)GetValue(EnableMaxRestoreProperty); }
      set { SetValue(EnableMaxRestoreProperty, value); }
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
      // no transparency, because it has more then one unwanted issues
      if (AssociatedObject.AllowsTransparency
          && AssociatedObject.IsLoaded == false
          && new WindowInteropHelper(AssociatedObject).Handle == IntPtr.Zero)
      {
        try
        {
          AssociatedObject.SetCurrentValue(Window.AllowsTransparencyProperty, false);
        }
        catch (Exception)
        {
          //For some reason, we can't determine if the window has loaded or not, so we swallow the exception.
        }
      }

      if (AssociatedObject.WindowStyle != WindowStyle.None)
      {
        AssociatedObject.SetCurrentValue(Window.WindowStyleProperty, WindowStyle.None);
      }

      savedBorderThickness = AssociatedObject.BorderThickness;
      borderThicknessChangeNotifier = new PropertyChangeNotifier(AssociatedObject, Control.BorderThicknessProperty);
      borderThicknessChangeNotifier.ValueChanged += BorderThicknessChangeNotifierOnValueChanged;

      savedResizeBorderThickness = ResizeBorderThickness;
      resizeBorderThicknessChangeNotifier = new PropertyChangeNotifier(this, ResizeBorderThicknessProperty);
      resizeBorderThicknessChangeNotifier.ValueChanged += ResizeBorderThicknessChangeNotifierOnValueChanged;

      savedTopMost = AssociatedObject.Topmost;
      topMostChangeNotifier = new PropertyChangeNotifier(AssociatedObject, Window.TopmostProperty);
      topMostChangeNotifier.ValueChanged += TopMostChangeNotifierOnValueChanged;

      AssociatedObject.SourceInitialized += AssociatedObject_SourceInitialized;
      AssociatedObject.Loaded += AssociatedObject_Loaded;
      AssociatedObject.Unloaded += AssociatedObject_Unloaded;
      AssociatedObject.Closed += AssociatedObject_Closed;
      AssociatedObject.StateChanged += AssociatedObject_StateChanged;
      AssociatedObject.LostFocus += AssociatedObject_LostFocus;
      AssociatedObject.Deactivated += AssociatedObject_Deactivated;

      base.OnAttached();
    }

    private void TopMostHack()
    {
      if (AssociatedObject.Topmost)
      {
        var raiseValueChanged = topMostChangeNotifier!.RaiseValueChanged;
        topMostChangeNotifier.RaiseValueChanged = false;
        AssociatedObject.SetCurrentValue(Window.TopmostProperty, false);
        AssociatedObject.SetCurrentValue(Window.TopmostProperty, true);
        topMostChangeNotifier.RaiseValueChanged = raiseValueChanged;
      }
    }

    /// <summary>
    /// Gets the default resize border thicknes from the system parameters.
    /// </summary>
    public static Thickness GetDefaultResizeBorderThickness()
    {
      return SystemParameters.WindowResizeBorderThickness;
    }

    private void BorderThicknessChangeNotifierOnValueChanged(object? sender, EventArgs e)
    {
      // It's bad if the window is null at this point, but we check this here to prevent the possible occurred exception
      var window = AssociatedObject;
      if (window is not null)
      {
        savedBorderThickness = window.BorderThickness;
      }
    }

    private void ResizeBorderThicknessChangeNotifierOnValueChanged(object? sender, EventArgs e)
    {
      savedResizeBorderThickness = ResizeBorderThickness;
    }

    private void TopMostChangeNotifierOnValueChanged(object? sender, EventArgs e)
    {
      // It's bad if the window is null at this point, but we check this here to prevent the possible occurred exception
      var window = AssociatedObject;
      if (window is not null)
      {
        savedTopMost = window.Topmost;
      }
    }

    private static void OnIgnoreTaskbarOnMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var behavior = (WindowChromeBehavior)d;
      behavior._OnChromePropertyChangedThatRequiresRepaint();

      // A few things to consider when removing the below hack
      // - ResizeMode="NoResize"
      //   WindowState="Maximized"
      //   IgnoreTaskbarOnMaximize="True"
      // - Changing IgnoreTaskbarOnMaximize while window is maximized

      // Changing the WindowState solves all, known, issues with changing IgnoreTaskbarOnMaximize.
      // Since IgnoreTaskbarOnMaximize is not changed all the time this hack seems to be less risky than anything else.
      if (behavior.AssociatedObject?.WindowState == WindowState.Maximized)
      {
        behavior.AssociatedObject.SetCurrentValue(Window.WindowStateProperty, WindowState.Normal);
        behavior.AssociatedObject.SetCurrentValue(Window.WindowStateProperty, WindowState.Maximized);
      }
    }

    private static void OnKeepBorderOnMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var behavior = (WindowChromeBehavior)d;

      behavior.HandleMaximize();
    }

    private static void OnTryToBeFlickerFreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var behavior = (WindowChromeBehavior)d;

      behavior._OnChromePropertyChangedThatRequiresRepaint();
    }

    [SecuritySafeCritical]
    private void Cleanup(bool isClosing)
    {
      if (isCleanedUp)
      {
        return;
      }

      isCleanedUp = true;

      OnCleanup();

      // clean up events
      AssociatedObject.SourceInitialized -= AssociatedObject_SourceInitialized;
      AssociatedObject.Loaded -= AssociatedObject_Loaded;
      AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
      AssociatedObject.Closed -= AssociatedObject_Closed;
      AssociatedObject.StateChanged -= AssociatedObject_StateChanged;
      AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
      AssociatedObject.Deactivated -= AssociatedObject_Deactivated;

      hwndSource?.RemoveHook(WindowProc);

      _RestoreStandardChromeState(isClosing);
    }

    /// <summary>
    /// Occurs during the cleanup of this behavior.
    /// </summary>
    protected virtual void OnCleanup()
    {
      // nothing here
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
      Cleanup(false);

      base.OnDetaching();
    }

    private void AssociatedObject_SourceInitialized(object? sender, EventArgs e)
    {
      windowHandle = new WindowInteropHelper(AssociatedObject).Handle;

      if (windowHandle == IntPtr.Zero)
      {
        throw new Exception("Uups, at this point we really need the Handle from the associated object!");
      }

      if (AssociatedObject.SizeToContent != SizeToContent.Manual
          && AssociatedObject.WindowState == WindowState.Normal)
      {
        // Another try to fix SizeToContent
        // without this we get nasty glitches at the borders
        Invoke(AssociatedObject, () =>
                                      {
                                        AssociatedObject.InvalidateMeasure();
                                                //                                                  if (UnsafeNativeMethods.GetWindowRect(this.windowHandle, out var rect))
                                                //                                                  {
                                                //                                                      var flags = SWP.SHOWWINDOW;
                                                //                                                      if (!this.AssociatedObject.ShowActivated)
                                                //                                                      {
                                                //                                                          flags |= SWP.NOACTIVATE;
                                                //                                                      }
                                                //                                                      NativeMethods.SetWindowPos(this.windowHandle, Constants.HWND_NOTOPMOST, rect.Left, rect.Top, rect.Width, rect.Height, flags);
                                                //                                                  }
                                              });
      }

      hwndSource = HwndSource.FromHwnd(windowHandle);
      hwndSource?.AddHook(WindowProc);

      _ApplyNewCustomChrome();

      // handle the maximized state here too (to handle the border in a correct way)
      HandleMaximize();
    }

    /// <summary>
    /// Is called when the associated object of this instance is loaded
    /// </summary>
    protected virtual void AssociatedObject_Loaded(object? sender, RoutedEventArgs e)
    {
      //this._UpdateFrameState(true);
    }

    private void AssociatedObject_Unloaded(object? sender, RoutedEventArgs e)
    {
      Cleanup(false);
    }

    private void AssociatedObject_Closed(object? sender, EventArgs e)
    {
      Cleanup(true);
    }

    private void AssociatedObject_StateChanged(object? sender, EventArgs e)
    {
      HandleMaximize();
    }

    private void AssociatedObject_Deactivated(object? sender, EventArgs e)
    {
      TopMostHack();
    }

    private void AssociatedObject_LostFocus(object? sender, RoutedEventArgs e)
    {
      TopMostHack();
    }

    private void HandleMaximize()
    {
      var raiseValueChanged = topMostChangeNotifier!.RaiseValueChanged;
      topMostChangeNotifier.RaiseValueChanged = false;

      HandleBorderAndResizeBorderThicknessDuringMaximize();

      if (AssociatedObject.WindowState == WindowState.Maximized)
      {
        // Workaround for:
        // MaxWidth="someValue"
        // SizeToContent = "WidthAndHeight"
        // Dragging the window to the top with those things set does not change the height of the Window
        if (AssociatedObject.SizeToContent != SizeToContent.Manual)
        {
          AssociatedObject.SetCurrentValue(Window.SizeToContentProperty, SizeToContent.Manual);
        }

        if (windowHandle != IntPtr.Zero)
        {
          // WindowChrome handles the size false if the main monitor is lesser the monitor where the window is maximized
          // so set the window pos/size twice
          var monitor = UnsafeNativeMethods.MonitorFromWindow(windowHandle, MonitorOptions.MONITOR_DEFAULTTONEAREST);
          if (monitor != IntPtr.Zero)
          {
            var monitorInfo = NativeMethods.GetMonitorInfo(monitor);
            var monitorRect = IgnoreTaskbarOnMaximize ? monitorInfo.rcMonitor : monitorInfo.rcWork;

            var x = monitorRect.Left;
            var y = monitorRect.Top;
            var cx = monitorRect.Width;
            var cy = monitorRect.Height;

            NativeMethods.SetWindowPos(windowHandle, Constants.HWND_NOTOPMOST, x, y, cx, cy, SWP.SHOWWINDOW);
          }
        }
      }

      // fix nasty TopMost bug
      // - set TopMost="True"
      // - start demo
      // - TopMost works
      // - maximize window and back to normal
      // - TopMost is gone
      //
      // Problem with minimize animation when window is maximized #1528
      // 1. Activate another application (such as Google Chrome).
      // 2. Run the demo and maximize it.
      // 3. Minimize the demo by clicking on the taskbar button.
      // Note that the minimize animation in this case does actually run, but somehow the other
      // application (Google Chrome in this example) is instantly switched to being the top window,
      // and so blocking the animation view.
      AssociatedObject.SetCurrentValue(Window.TopmostProperty, false);
      AssociatedObject.SetCurrentValue(Window.TopmostProperty, AssociatedObject.WindowState == WindowState.Minimized || savedTopMost);

      topMostChangeNotifier.RaiseValueChanged = raiseValueChanged;
    }

    /// <summary>
    /// This fix is needed because style triggers don't work if someone sets the value locally/directly on the window.
    /// </summary>
    private void HandleBorderAndResizeBorderThicknessDuringMaximize()
    {
      borderThicknessChangeNotifier!.RaiseValueChanged = false;
      resizeBorderThicknessChangeNotifier!.RaiseValueChanged = false;

      if (AssociatedObject.WindowState == WindowState.Maximized)
      {
        var monitor = IntPtr.Zero;

        if (windowHandle != IntPtr.Zero)
        {
          monitor = UnsafeNativeMethods.MonitorFromWindow(windowHandle, MonitorOptions.MONITOR_DEFAULTTONEAREST);
        }

        if (monitor != IntPtr.Zero)
        {
          var monitorInfo = NativeMethods.GetMonitorInfo(monitor);
          var monitorRect = IgnoreTaskbarOnMaximize ? monitorInfo.rcMonitor : monitorInfo.rcWork;

          var rightBorderThickness = 0D;
          var bottomBorderThickness = 0D;

          if (KeepBorderOnMaximize
              && savedBorderThickness.HasValue)
          {
            // If the maximized window will have a width less than the monitor size, show the right border.
            if (AssociatedObject.MaxWidth < monitorRect.Width)
            {
              rightBorderThickness = savedBorderThickness.Value.Right;
            }

            // If the maximized window will have a height less than the monitor size, show the bottom border.
            if (AssociatedObject.MaxHeight < monitorRect.Height)
            {
              bottomBorderThickness = savedBorderThickness.Value.Bottom;
            }
          }

          // set window border, so we can move the window from top monitor position
          AssociatedObject.SetCurrentValue(Control.BorderThicknessProperty, new Thickness(0, 0, rightBorderThickness, bottomBorderThickness));
        }
        else // Can't get monitor info, so just remove all border thickness
        {
          AssociatedObject.SetCurrentValue(Control.BorderThicknessProperty, new Thickness(0));
        }

        SetCurrentValue(ResizeBorderThicknessProperty, new Thickness(0));
      }
      else
      {
        AssociatedObject.SetCurrentValue(Control.BorderThicknessProperty, savedBorderThickness.GetValueOrDefault(new Thickness(0)));

        var resizeBorderThickness = savedResizeBorderThickness.GetValueOrDefault(new Thickness(0));

        if (ResizeBorderThickness != resizeBorderThickness)
        {
          SetCurrentValue(ResizeBorderThicknessProperty, resizeBorderThickness);
        }
      }

      borderThicknessChangeNotifier.RaiseValueChanged = true;
      resizeBorderThicknessChangeNotifier.RaiseValueChanged = true;
    }

    private static void Invoke(DispatcherObject dispatcherObject, Action invokeAction)
    {
      if (dispatcherObject is null)
      {
        throw new ArgumentNullException(nameof(dispatcherObject));
      }

      if (invokeAction is null)
      {
        throw new ArgumentNullException(nameof(invokeAction));
      }

      if (dispatcherObject.Dispatcher.CheckAccess())
      {
        invokeAction();
      }
      else
      {
        dispatcherObject.Dispatcher.Invoke(invokeAction);
      }
    }

    private static readonly List<SystemParameterBoundProperty> boundProperties = new()
    {
      new SystemParameterBoundProperty { DependencyProperty = ResizeBorderThicknessProperty, SystemParameterPropertyName = nameof(SystemParameters.WindowResizeBorderThickness) },
    };
  }
}