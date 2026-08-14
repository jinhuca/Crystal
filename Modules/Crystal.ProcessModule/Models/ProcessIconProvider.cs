using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystal.ProcessModule.Models;

/// <summary>
/// Resolves the small shell icon for a process executable (the same icon Explorer/Task Manager
/// shows) and caches it by path. Extraction hits the disk and the shell, so it is done off the UI
/// thread and each result is frozen — a frozen <see cref="ImageSource"/> is safe to hand to bound
/// UI from any thread. The cache is keyed by executable path because most of the thousands of
/// per-poll rows share a handful of images (dozens of svchost.exe, chrome.exe, …); one extraction
/// per distinct path is enough.
/// </summary>
public sealed class ProcessIconProvider {
  // Path → icon (null = we tried and failed, so we don't retry a missing/locked file every poll).
  private readonly ConcurrentDictionary<string, ImageSource?> _cache =
      new(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// Returns the cached icon for <paramref name="executablePath"/>, extracting it on first request.
  /// Returns null when the path is empty (WMI couldn't read it) or the icon can't be extracted; the
  /// caller shows a generic placeholder in that case. Safe to call from a background thread.
  /// </summary>
  public ImageSource? GetIcon(string? executablePath) {
    if (string.IsNullOrWhiteSpace(executablePath)) return null;
    return _cache.GetOrAdd(executablePath, Extract);
  }

  private static ImageSource? Extract(string path) {
    var info = new SHFILEINFO();
    // SHGFI_ICON | SHGFI_SMALLICON asks the shell for the 16px icon, including per-exe embedded and
    // file-association icons. SHGFI_USEFILEATTRIBUTES lets it answer by extension alone if the file
    // is momentarily unreadable, so we still get a sensible icon instead of nothing.
    const uint SHGFI_ICON = 0x000000100;
    const uint SHGFI_SMALLICON = 0x000000001;
    const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

    IntPtr result = SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, ref info, (uint)Marshal.SizeOf<SHFILEINFO>(),
        SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES);
    if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero) return null;

    try {
      var source = Imaging.CreateBitmapSourceFromHIcon(
          info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
      source.Freeze(); // cross-thread-usable and immutable once handed to the UI
      return source;
    } catch {
      return null;
    } finally {
      // CreateBitmapSourceFromHIcon copies the pixels, so the native handle must be released here.
      DestroyIcon(info.hIcon);
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  private struct SHFILEINFO {
    public IntPtr hIcon;
    public int iIcon;
    public uint dwAttributes;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szDisplayName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
    public string szTypeName;
  }

  [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
  private static extern IntPtr SHGetFileInfo(
      string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

  [DllImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool DestroyIcon(IntPtr hIcon);
}
