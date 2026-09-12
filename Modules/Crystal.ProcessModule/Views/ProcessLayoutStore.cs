using System.IO;
using System.Text.Json;

namespace Crystal.ProcessModule.Views;

/// <summary>
/// The persisted list/detail split of the Processes view: the star weights of the master (process
/// list) and detail (metrics) columns. Both are star sizes, so only their ratio matters and it
/// survives a change in window width.
/// </summary>
public sealed class ProcessSplitLayout {
  public double MasterStar { get; set; }
  public double DetailStar { get; set; }
}

/// <summary>
/// Reads and writes the Processes list/detail split to a small JSON file under %AppData%\Crystal, so
/// the split the user dragged is restored on the next launch. All IO is best-effort: a missing or
/// corrupt file just yields no saved layout (the view keeps its equal-width default) rather than
/// throwing.
/// </summary>
public sealed class ProcessLayoutStore {
  private readonly string _path;

  public ProcessLayoutStore() {
    var dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Crystal");
    _path = Path.Combine(dir, "process-layout.json");
  }

  public ProcessSplitLayout? Load() {
    try {
      if (File.Exists(_path)) {
        var layout = JsonSerializer.Deserialize<ProcessSplitLayout>(File.ReadAllText(_path));
        // Guard against a zeroed/negative file leaving a column collapsed.
        if (layout is { MasterStar: > 0, DetailStar: > 0 }) return layout;
      }
    } catch {
      // Corrupt or unreadable layout file — fall back to the default split.
    }
    return null;
  }

  public void Save(ProcessSplitLayout layout) {
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
      File.WriteAllText(_path,
          JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true }));
    } catch {
      // Best-effort persistence; losing a layout write is not worth crashing over.
    }
  }
}
