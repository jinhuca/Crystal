using System.Runtime.InteropServices;

namespace Crystal.Service.Process;

/// <summary>
/// Enumerates top-level windows and collects the PIDs that own at least one visible, non-tool
/// window — i.e. processes a user would recognize as a running "app". Used to split the process
/// list into Apps vs. Background Processes the way Task Manager does.
/// </summary>
internal static class VisibleWindowScanner {
  /// <summary>
  /// Callback signature <see cref="EnumWindows"/> invokes once per top-level window; return
  /// true to keep enumerating.
  /// </summary>
  private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  /// <summary>
  /// Enumerates all top-level windows on the screen, calling <paramref name="lpEnumFunc"/> for each.
  /// </summary>
  [DllImport("user32.dll")]
  private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

  /// <summary>
  /// Whether the given window has the WS_VISIBLE style set.
  /// </summary>
  [DllImport("user32.dll")]
  private static extern bool IsWindowVisible(IntPtr hWnd);

  /// <summary>
  /// Length of the window's title text; used to cheaply skip untitled windows.
  /// </summary>
  [DllImport("user32.dll")]
  private static extern int GetWindowTextLength(IntPtr hWnd);

  /// <summary>
  /// Retrieves the PID that owns the window; the return value is the owning thread id.
  /// </summary>
  [DllImport("user32.dll", SetLastError = true)]
  private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

  /// <summary>
  /// Reads a window attribute (here the extended style bits) by index.
  /// </summary>
  [DllImport("user32.dll", SetLastError = true)]
  private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

  /// <summary>
  /// Index passed to <see cref="GetWindowLong"/> to read the extended window style.
  /// </summary>
  private const int GWL_EXSTYLE = -20;

  /// <summary>
  /// Extended-style bit marking a tool window (tooltip / floating palette), which is not an "app".
  /// </summary>
  private const int WS_EX_TOOLWINDOW = 0x00000080;

  /// <summary>
  /// PIDs that own a visible, titled, non-tool top-level window this instant.
  /// </summary>
  /// <returns>The set of process ids that currently own at least one qualifying window.</returns>
  public static HashSet<uint> GetPidsWithVisibleWindows() {
    var pids = new HashSet<uint>();

    EnumWindows((hWnd, _) => {
      if (!IsWindowVisible(hWnd)) return true;
      if (GetWindowTextLength(hWnd) == 0) return true;
      // Skip tool windows (tooltips, floating palettes) — they aren't "apps".
      if ((GetWindowLong(hWnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW) != 0) return true;

      if (GetWindowThreadProcessId(hWnd, out uint pid) != 0 && pid != 0) pids.Add(pid);
      return true;
    }, IntPtr.Zero);

    return pids;
  }
}
