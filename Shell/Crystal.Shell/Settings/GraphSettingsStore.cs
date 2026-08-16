using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Crystal.Shell.Settings;

/// <summary>
/// Reads and writes the dashboard graph-appearance selection (category + per-graph kind/accent) to a
/// small JSON file under <c>%AppData%\Crystal</c>, so the chosen look is restored on the next launch.
/// Mirrors <see cref="Crystal.Shell.Navigation.WindowLayoutStore"/>: all IO is best-effort — a
/// missing or corrupt file just yields defaults rather than throwing.
/// </summary>
public sealed class GraphSettingsStore {
  // Enums are written as their names (e.g. "NoFrills") so the file is human-readable and stable
  // against enum-value reordering.
  private static readonly JsonSerializerOptions JsonOptions = new() {
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
  };

  private readonly string _path;
  private GraphSettings _settings;

  public GraphSettingsStore() : this(DefaultDirectory()) { }

  // Directory is injectable so the persisted location can be redirected (e.g. in a test) without
  // touching the real user profile.
  public GraphSettingsStore(string directory) {
    _path = Path.Combine(directory, "graph-settings.json");
    _settings = Load();
  }

  private static string DefaultDirectory() => Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Crystal");

  private GraphSettings Load() {
    try {
      if (File.Exists(_path)) {
        var text = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<GraphSettings>(text, JsonOptions) ?? new();
      }
    } catch {
      // Corrupt or unreadable settings file — fall back to defaults rather than failing startup.
    }
    return new();
  }

  /// <summary>The current settings. Never null; defaults until something is saved.</summary>
  public GraphSettings Current => _settings;

  /// <summary>Raised after <see cref="Save"/> swaps in new settings, so live dashboard graphs can
  /// re-apply the selection without waiting for the next launch.</summary>
  public event Action? Changed;

  public void Save(GraphSettings settings) {
    _settings = settings;
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
      File.WriteAllText(_path, JsonSerializer.Serialize(_settings, JsonOptions));
    } catch {
      // Best-effort persistence; losing a settings write is not worth crashing over.
    }
    // Notify even if the write failed: the in-memory settings changed, so the live graphs should
    // reflect the new selection for the rest of the session regardless.
    Changed?.Invoke();
  }
}
