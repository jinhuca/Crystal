using Crystal.Themes.Behaviors;
using Crystal.Themes.Internal;
using Crystal.Themes.Native;
using Crystal.Themes.Standard;
using System.Runtime.InteropServices;
using System.Windows.Interop;

#pragma warning disable 618, CA1001, CA1060

namespace Crystal.Themes.Controls;

[DebuggerDisplay("{" + nameof(Title) + "}")]
public partial class GlowWindow
{
  private readonly Func<Point, RECT, HT> getHitTestValue;
  private readonly Func<RECT, double> getLeft;
  private readonly Func<RECT, double> getTop;
  private readonly Func<RECT, double> getWidth;
  private readonly Func<RECT, double> getHeight;

  private IntPtr windowHandle;
  private IntPtr ownerWindowHandle;
  private bool closing;
  private HwndSource? hwndSource;
  private PropertyChangeNotifier? resizeModeChangeNotifier;

  private readonly Window owner;

  private RECT lastUpdateCoreRect;

  #region PInvoke

  [DllImport("user32.dll")]
  private static extern IntPtr LoadCursor(IntPtr hInstance, IDC_SIZE_CURSORS cursor);

  [DllImport("user32.dll")]
  private static extern IntPtr SetCursor(IntPtr cursor);

  private enum IDC_SIZE_CURSORS
  {
    SIZENWSE = 32642,
    SIZENESW = 32643,
    SIZEWE = 32644,
    SIZENS = 32645,
  }

  #endregion

  static GlowWindow()
  {
    AllowsTransparencyProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(true));
    BackgroundProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(Brushes.Transparent));
    ResizeModeProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(ResizeMode.NoResize));
    ShowActivatedProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(false));
    ShowInTaskbarProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(false));
    SnapsToDevicePixelsProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(true));
    WindowStyleProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(WindowStyle.None));
  }

  public GlowWindow(Window owner, GlowWindowBehavior behavior, GlowDirection direction)
  {
    InitializeComponent();

    Title = $"GlowWindow_{direction}";
    Name = Title;

    // We have to set the owner to fix #92
    Owner = owner ?? throw new ArgumentNullException(nameof(owner));
    this.owner = owner;

    IsGlowing = true;
    Closing += (sender, e) => e.Cancel = !closing;

    glow.Direction = direction;

    {
      var b = new Binding
      {
        Path = new PropertyPath(nameof(ActualWidth)),
        Source = this,
        Mode = BindingMode.OneWay
      };
      glow.SetBinding(WidthProperty, b);
    }

    {
      var b = new Binding
      {
        Path = new PropertyPath(nameof(ActualHeight)),
        Source = this,
        Mode = BindingMode.OneWay
      };
      glow.SetBinding(HeightProperty, b);
    }

    {
      var b = new Binding
      {
        Path = new PropertyPath(GlowWindowBehavior.GlowBrushProperty),
        Source = behavior,
        Mode = BindingMode.OneWay
      };
      glow.SetBinding(Glow.GlowBrushProperty, b);
    }

    {
      var b = new Binding
      {
        Path = new PropertyPath(GlowWindowBehavior.NonActiveGlowBrushProperty),
        Source = behavior,
        Mode = BindingMode.OneWay
      };
      glow.SetBinding(Glow.NonActiveGlowBrushProperty, b);
    }

    {
      var b = new Binding
      {
        Path = new PropertyPath(BorderThicknessProperty),
        Source = owner,
        Mode = BindingMode.OneWay
      };
      glow.SetBinding(BorderThicknessProperty, b);
    }

    {
      var b = new Binding
      {
        Path = new PropertyPath(GlowWindowBehavior.ResizeBorderThicknessProperty),
        Source = behavior
      };
      SetBinding(ResizeBorderThicknessProperty, b);
    }

    switch (direction)
    {
      case GlowDirection.Left:
        glow.HorizontalAlignment = HorizontalAlignment.Right;
        getLeft = rect => rect.Left - ResizeBorderThickness.Left + 1;
        getTop = rect => rect.Top - (ResizeBorderThickness.Top / 2);
        getWidth = rect => ResizeBorderThickness.Left;
        getHeight = rect => rect.Height + ResizeBorderThickness.Top;
        getHitTestValue = (p, rect) => new Rect(0, 0, rect.Width, ResizeBorderThickness.Top * 2).Contains(p)
          ? HT.TOPLEFT
          : new Rect(0, rect.Height - ResizeBorderThickness.Bottom, rect.Width, ResizeBorderThickness.Bottom * 2).Contains(p)
            ? HT.BOTTOMLEFT
            : HT.LEFT;
        break;

      case GlowDirection.Right:
        glow.HorizontalAlignment = HorizontalAlignment.Left;
        getLeft = rect => rect.Right - 1;
        getTop = rect => rect.Top - (ResizeBorderThickness.Top / 2);
        getWidth = rect => ResizeBorderThickness.Right;
        getHeight = rect => rect.Height + ResizeBorderThickness.Top;
        getHitTestValue = (p, rect) => new Rect(0, 0, rect.Width, ResizeBorderThickness.Top * 2).Contains(p)
          ? HT.TOPRIGHT
          : new Rect(0, rect.Height - ResizeBorderThickness.Bottom, rect.Width, ResizeBorderThickness.Bottom * 2).Contains(p)
            ? HT.BOTTOMRIGHT
            : HT.RIGHT;
        break;

      case GlowDirection.Top:
        glow.VerticalAlignment = VerticalAlignment.Bottom;
        getLeft = rect => rect.Left - (ResizeBorderThickness.Left / 2);
        getTop = rect => rect.Top - ResizeBorderThickness.Top + 1;
        getWidth = rect => rect.Width + ResizeBorderThickness.Left;
        getHeight = rect => ResizeBorderThickness.Top;
        getHitTestValue = (p, rect) => new Rect(0, 0, ResizeBorderThickness.Left * 2, rect.Height).Contains(p)
          ? HT.TOPLEFT
          : new Rect(rect.Width - ResizeBorderThickness.Right, 0, ResizeBorderThickness.Right * 2, rect.Height).Contains(p)
            ? HT.TOPRIGHT
            : HT.TOP;
        break;

      case GlowDirection.Bottom:
        glow.VerticalAlignment = VerticalAlignment.Top;
        getLeft = rect => rect.Left - (ResizeBorderThickness.Left / 2);
        getTop = rect => rect.Bottom - 1;
        getWidth = rect => rect.Width + ResizeBorderThickness.Left;
        getHeight = rect => ResizeBorderThickness.Bottom;
        getHitTestValue = (p, rect) => new Rect(0, 0, ResizeBorderThickness.Left * 2, rect.Height).Contains(p)
          ? HT.BOTTOMLEFT
          : new Rect(rect.Width - ResizeBorderThickness.Right, 0, ResizeBorderThickness.Right * 2, rect.Height).Contains(p)
            ? HT.BOTTOMRIGHT
            : HT.BOTTOM;
        break;

      default:
        throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction.");
    }

    owner.Activated += OnOwnerActivated;
    owner.Deactivated += OnOwnerDeactivated;
    owner.IsVisibleChanged += OnOwnerIsVisibleChanged;
    owner.Closed += (sender, e) => InternalClose();
  }

  public bool IsGlowing { get; set; }

  public Storyboard? OpacityStoryboard { get; set; }

  public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(GlowWindow), new PropertyMetadata(WindowChromeBehavior.GetDefaultResizeBorderThickness(), OnResizeBorderThicknessChanged));

  public Thickness ResizeBorderThickness
  {
    get { return (Thickness)GetValue(ResizeBorderThicknessProperty); }
    set { SetValue(ResizeBorderThicknessProperty, value); }
  }

  private static void OnResizeBorderThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var glowWindow = (GlowWindow)d;

    // Add padding to the edges, otherwise the borders/glows overlap too much
    switch (glowWindow.glow.Direction)
    {
      case GlowDirection.Left:
      case GlowDirection.Right:
        glowWindow.glow.Padding = new Thickness(0, glowWindow.ResizeBorderThickness.Top / 4, 0, glowWindow.ResizeBorderThickness.Bottom / 4);
        break;

      case GlowDirection.Top:
      case GlowDirection.Bottom:
        glowWindow.glow.Padding = new Thickness(glowWindow.ResizeBorderThickness.Left / 4, 0, glowWindow.ResizeBorderThickness.Right / 4, 0);
        break;
    }

    glowWindow.Update();
  }

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    OpacityStoryboard = TryFindResource("Crystal.GlowWindow.OpacityStoryboard") as Storyboard;
  }

  protected override void OnSourceInitialized(EventArgs e)
  {
    base.OnSourceInitialized(e);

    hwndSource = (HwndSource)PresentationSource.FromVisual(this);

    if (hwndSource is null)
    {
      return;
    }

    windowHandle = hwndSource.Handle;
    var ownerWindowInteropHelper = new WindowInteropHelper(owner);
    ownerWindowHandle = ownerWindowInteropHelper.Handle;

    // Set parent to the owner of our owner, that way glows on modeless windows shown with an owner work correctly.
    // We must do that only in case our owner has an owner.
    if (ownerWindowInteropHelper.Owner != IntPtr.Zero)
    {
      NativeMethods.SetWindowLongPtr(windowHandle, GWL.HWNDPARENT, ownerWindowInteropHelper.Owner);
    }

    var ws = NativeMethods.GetWindowStyle(windowHandle);
    var wsex = NativeMethods.GetWindowStyleEx(windowHandle);

    ws &= ~WS.CAPTION; // We don't need a title bar
    ws &= ~WS.SYSMENU; // We don't need a system context menu
    ws |= WS.POPUP;

    wsex &= ~WS_EX.APPWINDOW; // We don't want our window to be visible on the taskbar
    wsex |= WS_EX.TOOLWINDOW; // We don't want our window to be visible on the taskbar
    wsex |= WS_EX.NOACTIVATE; // We don't want our this window to be activated

    if (owner.ResizeMode == ResizeMode.NoResize || owner.ResizeMode == ResizeMode.CanMinimize)
    {
      wsex |= WS_EX.TRANSPARENT;
    }

    NativeMethods.SetWindowStyle(windowHandle, ws);
    NativeMethods.SetWindowStyleEx(windowHandle, wsex);

    hwndSource.AddHook(WndProc);

    resizeModeChangeNotifier = new PropertyChangeNotifier(owner, ResizeModeProperty);
    resizeModeChangeNotifier.ValueChanged += ResizeModeChanged;
  }

  private void ResizeModeChanged(object? sender, EventArgs e)
  {
    var wsex = NativeMethods.GetWindowStyleEx(windowHandle);

    if (owner.ResizeMode == ResizeMode.NoResize || owner.ResizeMode == ResizeMode.CanMinimize)
    {
      wsex |= WS_EX.TRANSPARENT;
    }
    else
    {
      wsex &= ~WS_EX.TRANSPARENT;
    }

    NativeMethods.SetWindowStyleEx(windowHandle, wsex);
  }

  public void Update()
  {
    if (CanUpdateCore() == false)
    {
      return;
    }

    RECT rect;
    if (owner.Visibility == Visibility.Hidden)
    {
      InvokeIfCanUpdateCore(() =>
      {
        SetVisibilityIfPossible(Visibility.Collapsed);
      });

      if (IsGlowing
          && UnsafeNativeMethods.GetWindowRect(ownerWindowHandle, out rect))
      {
        UpdateCore(rect);
      }
    }
    else if (owner.WindowState == WindowState.Normal)
    {
      var newVisibility = IsGlowing
        ? Visibility.Visible
        : Visibility.Collapsed;

      InvokeIfCanUpdateCore(() =>
      {
        SetVisibilityIfPossible(newVisibility);
      });

      if (IsGlowing
          && UnsafeNativeMethods.GetWindowRect(ownerWindowHandle, out rect))
      {
        UpdateCore(rect);
      }
    }
    else
    {
      InvokeIfCanUpdateCore(() =>
      {
        SetVisibilityIfPossible(Visibility.Collapsed);
      });
    }
  }

  private void SetVisibilityIfPossible(Visibility newVisibility)
  {
    if (CanUpdateCore() == false)
    {
      return;
    }

    try
    {
      glow.Visibility = newVisibility;
      Visibility = newVisibility;
    }
    catch (Exception e)
    {
      Trace.TraceWarning($"Could not set Visibility: {e}");
#if DEBUG
      throw;
#endif
    }
  }

  private bool IsWindowHandleValid()
  {
    return WindowHelper.IsWindowHandleValid(windowHandle);
  }

  private bool IsOwnerHandleValid()
  {
    return WindowHelper.IsWindowHandleValid(ownerWindowHandle);
  }

  internal bool CanUpdateCore()
  {
    return closing == false
           && IsWindowHandleValid()
           && IsOwnerHandleValid();
  }

  internal void UpdateCore(RECT rect)
  {
    if (lastUpdateCoreRect.Equals(rect))
    {
      return;
    }

    lastUpdateCoreRect = rect;

    if (CanUpdateCore() == false)
    {
      return;
    }

    // we can handle this._owner.WindowState == WindowState.Normal
    // or use NOZORDER too
    // todo: direct z-order
    NativeMethods.SetWindowPos(windowHandle, ownerWindowHandle,
      (int)getLeft(rect),
      (int)getTop(rect),
      (int)getWidth(rect),
      (int)getHeight(rect),
      SWP.NOACTIVATE | SWP.NOZORDER);
  }

  private void OnOwnerActivated(object? sender, EventArgs e)
  {
    Update();

    glow.IsGlow = true;
  }

  private void OnOwnerDeactivated(object? sender, EventArgs e)
  {
    glow.IsGlow = false;
  }

  private void OnOwnerIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
  {
    Update();
  }

  internal void InternalClose()
  {
    closing = true;

    owner.Activated -= OnOwnerActivated;
    owner.Deactivated -= OnOwnerDeactivated;
    owner.IsVisibleChanged -= OnOwnerIsVisibleChanged;

    if (resizeModeChangeNotifier is not null)
    {
      resizeModeChangeNotifier.ValueChanged -= ResizeModeChanged;
      resizeModeChangeNotifier.Dispose();
    }

    if (hwndSource is not null)
    {
      hwndSource.RemoveHook(WndProc);
      hwndSource.Dispose();
    }

    Close();
  }

  private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
  {
    switch ((WM)msg)
    {
      case WM.SHOWWINDOW:
        if ((int)lParam == 3 && Visibility != Visibility.Visible) // 3 == SW_PARENTOPENING
        {
          handled = true; //handle this message so window isn't shown until we want it to       
        }
        else if (CanUpdateCore())
        {
          // todo: direct z-order
          // this fixes #58
          InvokeAsyncIfCanUpdateCore(DispatcherPriority.Send, FixWindowZOrder);
        }

        break;

      case WM.WINDOWPOSCHANGED:
      case WM.WINDOWPOSCHANGING:
        var wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS))!;
        wp.flags |= SWP.NOACTIVATE; // We always have to add SWP.NOACTIVATE to prevent accidental activation of this window
        // todo: direct z-order
        //wp.hwndInsertAfter = this.ownerWindowHandle;
        Marshal.StructureToPtr(wp, lParam, true);
        break;

      // When the window is shown as a modal window and we try to activate the owner of said window the window has to receive a series of NCACTIVATE messages.
      // But because we set the owner of the glow window we have to forward those messages to our owner.
      case WM.NCACTIVATE:
        handled = true;
        if (IsOwnerHandleValid())
        {
          NativeMethods.SendMessage(ownerWindowHandle, WM.NCACTIVATE, wParam, lParam);
        }

        // We have to return true according to https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-ncactivate
        // If we don't do that here the owner window can't be activated.
        return new IntPtr(1);

      case WM.ACTIVATE:
        handled = true;
        return IntPtr.Zero;

      case WM.MOUSEACTIVATE:
        handled = true;
        if (IsOwnerHandleValid())
        {
          NativeMethods.SendMessage(ownerWindowHandle, WM.ACTIVATE, wParam, lParam);
        }

        return new IntPtr(3) /* MA_NOACTIVATE */;

      case WM.NCLBUTTONDOWN:
      case WM.NCLBUTTONDBLCLK:
      case WM.NCRBUTTONDOWN:
      case WM.NCRBUTTONDBLCLK:
      case WM.NCMBUTTONDOWN:
      case WM.NCMBUTTONDBLCLK:
      case WM.NCXBUTTONDOWN:
      case WM.NCXBUTTONDBLCLK:
        handled = true;
        if (IsOwnerHandleValid())
        {
          // WA_CLICKACTIVE = 2
          NativeMethods.SendMessage(ownerWindowHandle, WM.ACTIVATE, new IntPtr(2), IntPtr.Zero);
          // Forward message to owner
          NativeMethods.PostMessage(ownerWindowHandle, (WM)msg, wParam, IntPtr.Zero);
        }

        break;

      case WM.NCHITTEST:
        if (owner.ResizeMode == ResizeMode.CanResize
            || owner.ResizeMode == ResizeMode.CanResizeWithGrip)
        {
          if (IsOwnerHandleValid()
              && UnsafeNativeMethods.GetWindowRect(ownerWindowHandle, out var rect))
          {
            if (NativeMethods.TryGetRelativeMousePosition(windowHandle, out var pt))
            {
              var hitTestValue = getHitTestValue(pt, rect);
              handled = true;
              return new IntPtr((int)hitTestValue);
            }
          }
        }

        break;

      case WM.SETCURSOR:
        switch ((HT)Utility.LOWORD((int)lParam))
        {
          case HT.LEFT:
          case HT.RIGHT:
            handled = true;
            SetCursor(LoadCursor(IntPtr.Zero, IDC_SIZE_CURSORS.SIZEWE));
            break;

          case HT.TOP:
          case HT.BOTTOM:
            handled = true;
            SetCursor(LoadCursor(IntPtr.Zero, IDC_SIZE_CURSORS.SIZENS));
            break;

          case HT.TOPLEFT:
          case HT.BOTTOMRIGHT:
            handled = true;
            SetCursor(LoadCursor(IntPtr.Zero, IDC_SIZE_CURSORS.SIZENWSE));
            break;

          case HT.TOPRIGHT:
          case HT.BOTTOMLEFT:
            handled = true;
            SetCursor(LoadCursor(IntPtr.Zero, IDC_SIZE_CURSORS.SIZENESW));
            break;
        }

        break;
    }

    return IntPtr.Zero;

    // this fixes #58
    void FixWindowZOrder()
    {
      NativeMethods.SetWindowPos(windowHandle, ownerWindowHandle, 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE | SWP.NOACTIVATE);
    }
  }

  private void InvokeIfCanUpdateCore(Action invokeAction)
  {
    if (CanUpdateCore() == false)
    {
      return;
    }

    if (Dispatcher.CheckAccess())
    {
      invokeAction();
    }
    else
    {
      Dispatcher.Invoke(invokeAction);
    }
  }

  private void InvokeAsyncIfCanUpdateCore(DispatcherPriority dispatcherPriority, Action invokeAction)
  {
    Dispatcher.BeginInvoke(dispatcherPriority, new Action(() =>
    {
      if (CanUpdateCore())
      {
        invokeAction();
      }
    }));
  }
}