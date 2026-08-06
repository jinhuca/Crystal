using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Crystal.Shell.Navigation;

/// <summary>Persisted placement of one detail window, keyed by its detail-view name.</summary>
public sealed class WindowLayout {
  public double Left { get; set; }
  public double Top { get; set; }
  public double Width { get; set; }
  public double Height { get; set; }
  public bool Topmost { get; set; }
  // True when the window was maximized at save time; restored windows re-maximize while keeping
  // the normal-state bounds above so "restore down" returns to the right rect.
  public bool Maximized { get; set; }
  // False for a layout that only carries a Topmost preference (window never moved/sized yet),
  // so we don't force a zero-origin, zero-size placement.
  public bool HasBounds { get; set; }
  // True while a window for this subsystem is open; persisted so the set of open detail
  // windows can be reopened on the next launch (session restore).
  public bool Open { get; set; }
}

/// <summary>
/// Reads and writes detail-window placement (position, size, always-on-top) to a small JSON
/// file under %AppData%\Crystal, so reopening a subsystem restores where the user left it.
/// All IO is best-effort: a missing or corrupt file just yields defaults rather than throwing.
/// </summary>
public sealed class WindowLayoutStore {
  private readonly string _path;
  private readonly Dictionary<string, WindowLayout> _layouts;

  public WindowLayoutStore() {
    var dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Crystal");
    _path = Path.Combine(dir, "window-layout.json");
    _layouts = Load();
  }

  private Dictionary<string, WindowLayout> Load() {
    try {
      if (File.Exists(_path)) {
        var text = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<Dictionary<string, WindowLayout>>(text) ?? new();
      }
    } catch {
      // Corrupt or unreadable layout file — fall back to defaults rather than failing startup.
    }
    return new();
  }

  public WindowLayout? Get(string key) => _layouts.TryGetValue(key, out var l) ? l : null;

  /// <summary>Detail-view names whose window was open when last persisted — the session-restore set.</summary>
  public IReadOnlyList<string> OpenKeys() {
    var keys = new List<string>();
    foreach (var (key, layout) in _layouts)
      if (layout.Open) keys.Add(key);
    return keys;
  }

  public void Save(string key, WindowLayout layout) {
    _layouts[key] = layout;
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
      File.WriteAllText(_path,
          JsonSerializer.Serialize(_layouts, new JsonSerializerOptions { WriteIndented = true }));
    } catch {
      // Best-effort persistence; losing a layout write is not worth crashing over.
    }
  }
}
