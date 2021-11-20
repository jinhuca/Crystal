using System.Runtime.InteropServices;
using System.Windows.Interop;
using Crystal.Behaviors;
using Crystal.Themes.Controls;
using Crystal.Themes.Internal;
using Crystal.Themes.Native;
using Crystal.Themes.Standard;

namespace Crystal.Themes.Behaviors
{

  public class GlowWindowBehavior : Behavior<Window>
  {
    private static readonly TimeSpan glowTimerDelay = TimeSpan.FromMilliseconds(200); //200 ms delay, the same as in visual studio
    private GlowWindow? left;
    private GlowWindow? right;
    private GlowWindow? top;
    private GlowWindow? bottom;
    private DispatcherTimer? makeGlowVisibleTimer;
    private IntPtr windowHandle;
    private HwndSource? hwndSource;

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="GlowBrush"/>.
    /// </summary>
    public static readonly DependencyProperty GlowBrushProperty = DependencyProperty.Register(nameof(GlowBrush), typeof(Brush), typeof(GlowWindowBehavior), new PropertyMetadata(default(Brush), OnGlowBrushChanged));

    private static void OnGlowBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is null
          || e.NewValue is null)
      {
        ((GlowWindowBehavior)d).Update();
      }
    }

    /// <summary>
    /// Gets or sets a brush which is used as the glow when the window is active.
    /// </summary>
    public Brush? GlowBrush
    {
      get => (Brush?)GetValue(GlowBrushProperty);
      set => SetValue(GlowBrushProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="NonActiveGlowBrush"/>.
    /// </summary>
    public static readonly DependencyProperty NonActiveGlowBrushProperty = DependencyProperty.Register(nameof(NonActiveGlowBrush), typeof(Brush), typeof(GlowWindowBehavior), new PropertyMetadata(default(Brush)));

    /// <summary>
    /// Gets or sets a brush which is used as the glow when the window is not active.
    /// </summary>
    public Brush? NonActiveGlowBrush
    {
      get => (Brush?)GetValue(NonActiveGlowBrushProperty);
      set => SetValue(NonActiveGlowBrushProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="IsGlowTransitionEnabled"/>.
    /// </summary>
    public static readonly DependencyProperty IsGlowTransitionEnabledProperty = DependencyProperty.Register(nameof(IsGlowTransitionEnabled), typeof(bool), typeof(GlowWindowBehavior), new PropertyMetadata(default(bool)));

    /// <summary>
    /// Defines whether glow transitions should be used or not.
    /// </summary>
    public bool IsGlowTransitionEnabled
    {
      get => (bool)GetValue(IsGlowTransitionEnabledProperty);
      set => SetValue(IsGlowTransitionEnabledProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="ResizeBorderThickness"/>.
    /// </summary>
    public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(GlowWindowBehavior), new PropertyMetadata(default(Thickness)));

    /// <summary>
    /// Gets or sets resize border thickness.
    /// </summary>
    public Thickness ResizeBorderThickness
    {
      get => (Thickness)GetValue(ResizeBorderThicknessProperty);
      set => SetValue(ResizeBorderThicknessProperty, value);
    }

    private bool IsActiveGlowDisabled => GlowBrush is null;

    private bool IsNoneActiveGlowDisabled => NonActiveGlowBrush is null;

    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.SourceInitialized += AssociatedObjectSourceInitialized;
      AssociatedObject.Loaded += AssociatedObjectOnLoaded;
      AssociatedObject.Unloaded += AssociatedObjectUnloaded;

      if (AssociatedObject.IsLoaded)
      {
        AssociatedObjectOnLoaded(AssociatedObject, new RoutedEventArgs());
        AssociatedObjectSourceInitialized(AssociatedObject, EventArgs.Empty);
      }
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
      AssociatedObject.SourceInitialized -= AssociatedObjectSourceInitialized;
      AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
      AssociatedObject.Unloaded -= AssociatedObjectUnloaded;

      hwndSource?.RemoveHook(AssociatedObjectWindowProc);

      AssociatedObject.Activated -= AssociatedObjectActivatedOrDeactivated;
      AssociatedObject.Deactivated -= AssociatedObjectActivatedOrDeactivated;
      AssociatedObject.StateChanged -= AssociatedObjectStateChanged;
      AssociatedObject.IsVisibleChanged -= AssociatedObjectIsVisibleChanged;
      AssociatedObject.Closing -= AssociatedObjectOnClosing;

      DestroyGlowVisibleTimer();

      Close();

      base.OnDetaching();
    }

    private void AssociatedObjectSourceInitialized(object? sender, EventArgs e)
    {
      windowHandle = new WindowInteropHelper(AssociatedObject).Handle;
      hwndSource = HwndSource.FromHwnd(windowHandle);
      hwndSource?.AddHook(AssociatedObjectWindowProc);
    }

    private void AssociatedObjectStateChanged(object? sender, EventArgs e)
    {
      makeGlowVisibleTimer?.Stop();

      if (AssociatedObject.WindowState == WindowState.Normal)
      {
        var ignoreTaskBar = Interaction.GetBehaviors(AssociatedObject).OfType<WindowChromeBehavior>().FirstOrDefault()?.IgnoreTaskbarOnMaximize == true;
        if (makeGlowVisibleTimer is not null
            && SystemParameters.MinimizeAnimation
            && !ignoreTaskBar)
        {
          makeGlowVisibleTimer.Start();
        }
        else
        {
          RestoreGlow();
        }
      }
      else
      {
        HideGlow();
      }
    }

    private void AssociatedObjectUnloaded(object? sender, RoutedEventArgs e)
    {
      DestroyGlowVisibleTimer();
    }

    private void DestroyGlowVisibleTimer()
    {
      if (makeGlowVisibleTimer is null)
      {
        return;
      }

      makeGlowVisibleTimer.Stop();
      makeGlowVisibleTimer.Tick -= GlowVisibleTimerOnTick;
      makeGlowVisibleTimer = null;
    }

    private void GlowVisibleTimerOnTick(object? sender, EventArgs e)
    {
      makeGlowVisibleTimer?.Stop();
      RestoreGlow();
    }

    private void RestoreGlow()
    {
      if (left is not null)
      {
        left.IsGlowing = true;
      }

      if (top is not null)
      {
        top.IsGlowing = true;
      }

      if (right is not null)
      {
        right.IsGlowing = true;
      }

      if (bottom is not null)
      {
        bottom.IsGlowing = true;
      }

      Update();
    }

    private void HideGlow()
    {
      if (left is not null)
      {
        left.IsGlowing = false;
      }

      if (top is not null)
      {
        top.IsGlowing = false;
      }

      if (right is not null)
      {
        right.IsGlowing = false;
      }

      if (bottom is not null)
      {
        bottom.IsGlowing = false;
      }

      Update();
    }

    private void AssociatedObjectOnLoaded(object? sender, RoutedEventArgs routedEventArgs)
    {
      // No glow effect if GlowBrush not set.
      if (IsActiveGlowDisabled)
      {
        return;
      }

      AssociatedObject.Activated -= AssociatedObjectActivatedOrDeactivated;
      AssociatedObject.Activated += AssociatedObjectActivatedOrDeactivated;
      AssociatedObject.Deactivated -= AssociatedObjectActivatedOrDeactivated;
      AssociatedObject.Deactivated += AssociatedObjectActivatedOrDeactivated;

      AssociatedObject.StateChanged -= AssociatedObjectStateChanged;
      AssociatedObject.StateChanged += AssociatedObjectStateChanged;

      if (makeGlowVisibleTimer is null)
      {
        makeGlowVisibleTimer = new DispatcherTimer { Interval = glowTimerDelay };
        makeGlowVisibleTimer.Tick += GlowVisibleTimerOnTick;
      }

      left = new GlowWindow(AssociatedObject, this, GlowDirection.Left);
      right = new GlowWindow(AssociatedObject, this, GlowDirection.Right);
      top = new GlowWindow(AssociatedObject, this, GlowDirection.Top);
      bottom = new GlowWindow(AssociatedObject, this, GlowDirection.Bottom);

      loadedGlowWindows = new[]
                               {
                                         left,
                                         top,
                                         right,
                                         bottom
                                     };

      Show();
      Update();

      if (!IsGlowTransitionEnabled)
      {
        // no storyboard so set opacity to 1
        AssociatedObject.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => SetOpacityTo(1)));
      }
      else
      {
        // start the opacity storyboard 0->1
        StartOpacityStoryboard();
        // hide the glows if window get invisible state
        AssociatedObject.IsVisibleChanged += AssociatedObjectIsVisibleChanged;
        // closing always handled
        AssociatedObject.Closing += AssociatedObjectOnClosing;
      }
    }

    private void AssociatedObjectOnClosing(object? o, CancelEventArgs args)
    {
      if (!args.Cancel)
      {
        AssociatedObject.IsVisibleChanged -= AssociatedObjectIsVisibleChanged;
      }
    }

#pragma warning disable 618
    private WINDOWPOS prevWindowPos;
    private GlowWindow[] loadedGlowWindows = new GlowWindow[0];
    private bool updatingZOrder;

    private IntPtr AssociatedObjectWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (hwndSource?.RootVisual is null)
      {
        return IntPtr.Zero;
      }

      switch ((WM)msg)
      {
        // Z-Index must NOT be updated when WINDOWPOSCHANGING
        case WM.WINDOWPOSCHANGING:
          {
            Assert.IsNotDefault(lParam);
            var wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS))!;
            if (wp.SizeAndPositionEquals(prevWindowPos) == false)
            {
              UpdateCore();
              prevWindowPos = wp;
            }
          }

          break;

        // Z-Index must be updated when WINDOWPOSCHANGED
        case WM.WINDOWPOSCHANGED:
          {
            Assert.IsNotDefault(lParam);
            var wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS))!;
            if (wp.SizeAndPositionEquals(prevWindowPos) == false)
            {
              UpdateCore();
              prevWindowPos = wp;
            }

            // todo: direct z-order
            //foreach (GlowWindow loadedGlowWindow in this.loadedGlowWindows)
            //{
            //    var glowWindowHandle = new WindowInteropHelper(loadedGlowWindow).Handle;
            //    NativeMethods.SetWindowPos(glowWindowHandle, this.windowHandle, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);
            //}

            UpdateZOrderOfThisAndOwner();
          }

          break;

        case WM.SIZE:
        case WM.SIZING:
          UpdateCore();
          break;
      }

      return IntPtr.Zero;
    }

    #region Z-Order

    private void UpdateZOrderOfThisAndOwner()
    {
      if (updatingZOrder)
      {
        return;
      }

      try
      {
        updatingZOrder = true;
        var windowInteropHelper = new WindowInteropHelper(AssociatedObject);
        var currentHandle = windowInteropHelper.Handle;

        if (currentHandle != windowHandle
            || WindowHelper.IsWindowHandleValid(currentHandle) == false)
        {
          return;
        }

        foreach (var loadedGlowWindow in loadedGlowWindows)
        {
          var glowWindowHandle = new WindowInteropHelper(loadedGlowWindow).Handle;

          var window = NativeMethods.GetWindow(glowWindowHandle, GW.HWNDPREV);
          if (window != currentHandle)
          {
            if (WindowHelper.IsWindowHandleValid(glowWindowHandle)
                && WindowHelper.IsWindowHandleValid(currentHandle))
            {
              NativeMethods.SetWindowPos(glowWindowHandle, currentHandle, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);
            }
          }

          currentHandle = glowWindowHandle;
        }

        UpdateZOrderOfOwner(currentHandle, windowInteropHelper.Owner);
      }
      finally
      {
        updatingZOrder = false;
      }
    }

    private void UpdateZOrderOfOwner(IntPtr hwndWindow, IntPtr hwndOwner)
    {
      if (WindowHelper.IsWindowHandleValid(hwndWindow) == false
          || WindowHelper.IsWindowHandleValid(hwndOwner) == false)
      {
        return;
      }

      var lastOwnedWindow = IntPtr.Zero;
      NativeMethods.EnumThreadWindows(NativeMethods.GetCurrentThreadId(), delegate (IntPtr hwnd, IntPtr lParam)
                                                                          {
                                                                            if (NativeMethods.GetWindow(hwnd, GW.OWNER) == hwndOwner)
                                                                            {
                                                                              lastOwnedWindow = hwnd;
                                                                            }

                                                                            return true;
                                                                          }, IntPtr.Zero);
      if (WindowHelper.IsWindowHandleValid(hwndOwner)
          && WindowHelper.IsWindowHandleValid(lastOwnedWindow)
          && NativeMethods.GetWindow(hwndOwner, GW.HWNDPREV) != lastOwnedWindow)
      {
        NativeMethods.SetWindowPos(hwndOwner, lastOwnedWindow, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);
      }
    }
#pragma warning restore 618

    #endregion

    private void AssociatedObjectActivatedOrDeactivated(object? sender, EventArgs e)
    {
      UpdateCore();
    }

    private void AssociatedObjectIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
      if (!AssociatedObject.IsVisible)
      {
        // the associated owner got invisible so set opacity to 0 to start the storyboard by 0 for the next visible state
        SetOpacityTo(0);
      }
      else
      {
        StartOpacityStoryboard();
      }
    }

    /// <summary>
    /// Updates all glow windows (visible, hidden, collapsed)
    /// </summary>
    private void Update()
    {
      left?.Update();
      right?.Update();
      top?.Update();
      bottom?.Update();
    }

#pragma warning disable 618
    private void UpdateCore()
    {
      if ((IsActiveGlowDisabled && AssociatedObject.IsActive)
          || (IsNoneActiveGlowDisabled && AssociatedObject.IsActive == false)
          || WindowHelper.IsWindowHandleValid(windowHandle) == false
          || NativeMethods.IsWindowVisible(windowHandle) == false)
      {
        return;
      }

      if (UnsafeNativeMethods.GetWindowRect(windowHandle, out var rect))
      {
        left?.UpdateCore(rect);
        right?.UpdateCore(rect);
        top?.UpdateCore(rect);
        bottom?.UpdateCore(rect);
      }
    }
#pragma warning restore 618

    /// <summary>
    /// Sets the opacity to all glow windows
    /// </summary>
    private void SetOpacityTo(double newOpacity)
    {
      var canSetOpacity = left is not null
                          && right is not null
                          && top is not null
                          && bottom is not null;

      if (canSetOpacity)
      {
        left!.Opacity = newOpacity;
        right!.Opacity = newOpacity;
        top!.Opacity = newOpacity;
        bottom!.Opacity = newOpacity;
      }
    }

    /// <summary>
    /// Starts the opacity storyboard 0 -> 1
    /// </summary>
    private void StartOpacityStoryboard()
    {
      var canStartOpacityStoryboard = left?.OpacityStoryboard is not null
                                      && right?.OpacityStoryboard is not null
                                      && top?.OpacityStoryboard is not null
                                      && bottom?.OpacityStoryboard is not null;

      if (canStartOpacityStoryboard)
      {
        left!.BeginStoryboard(left.OpacityStoryboard!);
        right!.BeginStoryboard(right.OpacityStoryboard!);
        top!.BeginStoryboard(top.OpacityStoryboard!);
        bottom!.BeginStoryboard(bottom.OpacityStoryboard!);
      }
    }

    /// <summary>
    /// Shows all glow windows
    /// </summary>
    private void Show()
    {
      left?.Show();
      right?.Show();
      top?.Show();
      bottom?.Show();
    }

    /// <summary>
    /// Closes all glow windows
    /// </summary>
    private void Close()
    {
      left?.InternalClose();
      right?.InternalClose();
      top?.InternalClose();
      bottom?.InternalClose();
    }
  }
}