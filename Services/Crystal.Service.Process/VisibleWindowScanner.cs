using System.Runtime.InteropServices;

namespace Crystal.Service.Process;

/// <summary>
/// Enumerates top-level windows and collects the PIDs that own at least one visible, non-tool
/// window — i.e. processes a user would recognize as a running "app". Used to split the process
/// list into Apps vs. Background Processes the way Task Manager does.
/// </summary>
internal static class VisibleWindowScanner {
  private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

  [DllImport("user32.dll")]
  private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

  [DllImport("user32.dll")]
  private static extern bool IsWindowVisible(IntPtr hWnd);

  [DllImport("user32.dll")]
  private static extern int GetWindowTextLength(IntPtr hWnd);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

  private const int GWL_EXSTYLE = -20;
  private const int WS_EX_TOOLWINDOW = 0x00000080;

  /// <summary>PIDs that own a visible, titled, non-tool top-level window this instant.</summary>
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
