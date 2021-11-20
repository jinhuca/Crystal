using System.Windows.Interop;
using Crystal.Behaviors;
using Crystal.Themes.Controls;
using Crystal.Themes.Native;
using Crystal.Themes.Standard;

namespace Crystal.Themes.Behaviors
{
  public class WindowsSettingBehavior : Behavior<CrystalWindow>
  {
    /// <inheritdoc/>
    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.SourceInitialized += AssociatedObject_SourceInitialized;
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
      CleanUp("from OnDetaching");
      base.OnDetaching();
    }

    private void AssociatedObject_SourceInitialized(object? sender, EventArgs e)
    {
      LoadWindowState();

      var window = AssociatedObject;
      if (window is null)
      {
        // if the associated object is null at this point, then there is really something wrong!
        Trace.TraceError($"{this}: Can not attach to nested events, cause the AssociatedObject is null.");
        return;
      }

      window.StateChanged += AssociatedObject_StateChanged;
      window.Closing += AssociatedObject_Closing;
      window.Closed += AssociatedObject_Closed;

      // This operation must be thread safe. It is possible, that the window is running in a different Thread.
      Application.Current?.BeginInvoke(app =>
          {
            if (app != null)
            {
              app.SessionEnding += CurrentApplicationSessionEnding;
            }
          });
    }

    private void AssociatedObject_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveWindowState();
    }

    private void AssociatedObject_Closed(object? sender, EventArgs e)
    {
      CleanUp("from AssociatedObject closed event");
    }

    private void CurrentApplicationSessionEnding(object? sender, SessionEndingCancelEventArgs e)
    {
      SaveWindowState();
    }

    private void AssociatedObject_StateChanged(object? sender, EventArgs e)
    {
      // save the settings on this state change, because hidden windows gets no window placements
      // all the saving stuff could be so much easier with ReactiveUI :-D
      if (AssociatedObject?.WindowState == WindowState.Minimized)
      {
        SaveWindowState();
      }
    }

    private void CleanUp(string fromWhere)
    {
      var window = AssociatedObject;
      if (window is null)
      {
        // it's bad if the associated object is null, so trace this here
        Trace.TraceWarning($"{this}: Can not clean up {fromWhere}, cause the AssociatedObject is null. This can maybe happen if this Behavior was already detached.");
        return;
      }

      Debug.WriteLine($"{this}: Clean up {fromWhere}.");

      window.StateChanged -= AssociatedObject_StateChanged;
      window.Closing -= AssociatedObject_Closing;
      window.Closed -= AssociatedObject_Closed;
      window.SourceInitialized -= AssociatedObject_SourceInitialized;

      // This operation must be thread safe
      Application.Current?.BeginInvoke(app =>
          {
            if (app != null)
            {
              app.SessionEnding -= CurrentApplicationSessionEnding;
            }
          });
    }

#pragma warning disable 618
    private void LoadWindowState()
    {
      var window = AssociatedObject;
      if (window is null)
      {
        return;
      }

      var settings = window.GetWindowPlacementSettings();
      if (settings is null || !window.SaveWindowPosition)
      {
        return;
      }

      try
      {
        settings.Reload();
      }
      catch (Exception e)
      {
        Trace.TraceError($"{this}: The settings could not be reloaded! {e}");
        return;
      }

      // check for existing placement and prevent empty bounds
      if (settings.Placement is null || settings.Placement.normalPosition.IsEmpty)
      {
        return;
      }

      try
      {
        var wp = settings.Placement;
        wp.flags = 0;
        wp.showCmd = (wp.showCmd == SW.SHOWMINIMIZED ? SW.SHOWNORMAL : wp.showCmd);

        // this fixes wrong monitor positioning together with different Dpi usage for SetWindowPlacement
        window.Left = wp.normalPosition.Left;
        window.Top = wp.normalPosition.Top;

        var windowHandle = new WindowInteropHelper(window).Handle;
        if (!NativeMethods.SetWindowPlacement(windowHandle, wp))
        {
          Trace.TraceWarning($"{this}: The WINDOWPLACEMENT {wp} could not be set by SetWindowPlacement.");
        }
      }
      catch (Exception ex)
      {
        throw new CrystalThemesException("Failed to set the window state from the settings file", ex);
      }
    }

    private void SaveWindowState()
    {
      var window = AssociatedObject;
      if (window is null)
      {
        return;
      }

      var settings = window.GetWindowPlacementSettings();
      if (settings is null || !window.SaveWindowPosition)
      {
        return;
      }

      var windowHandle = new WindowInteropHelper(window).Handle;
      var wp = NativeMethods.GetWindowPlacement(windowHandle);

      // check for saveable values
      if (wp.showCmd != SW.HIDE && wp.length > 0)
      {
        if (wp.showCmd == SW.NORMAL)
        {
          if (UnsafeNativeMethods.GetWindowRect(windowHandle, out var rect))
          {
            var monitor = NativeMethods.MonitorFromWindow(windowHandle, MonitorOptions.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
              var monitorInfo = NativeMethods.GetMonitorInfo(monitor);
              rect.Offset(monitorInfo.rcMonitor.Left - monitorInfo.rcWork.Left, monitorInfo.rcMonitor.Top - monitorInfo.rcWork.Top);
            }

            wp.normalPosition = rect;
          }
        }

        if (!wp.normalPosition.IsEmpty)
        {
          settings.Placement = wp;
        }
      }

      try
      {
        settings.Save();
      }
      catch (Exception e)
      {
        Trace.TraceError($"{this}: The settings could not be saved! {e}");
      }
    }
#pragma warning restore 618
  }
}